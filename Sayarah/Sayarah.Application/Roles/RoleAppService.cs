using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Providers;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.Roles
{
    [AbpAuthorize]
    public class RoleAppService : AsyncCrudAppService<Role, RoleDto, int, PagedResultRequestDto, CreateRoleDto, RoleDto>, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly PermissionManager _permissionManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;

        private readonly IRepository<MainProvider, long> _mainProviderRepository;

        public RoleAppService(
            IRepository<Role> repository,
            RoleManager roleManager,
            UserManager userManager,
            PermissionManager permissionManager,
            IRepository<User, long> userRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IRepository<MainProvider, long> mainProviderRepository
            )
            : base(repository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _permissionManager = permissionManager;

            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _mainProviderRepository = mainProviderRepository;
        }

        public override async Task<RoleDto> CreateAsync(CreateRoleDto input)
        {
            CheckCreatePermission();

            var role = ObjectMapper.Map<Role>(input);

            CheckErrors(await _roleManager.CreateAsync(role));

            UnitOfWorkManager.Current.SaveChanges();

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.Permissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return MapToEntityDto(role);
        }

        public override async Task<RoleDto> UpdateAsync(RoleDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            ObjectMapper.Map(input, role);

            CheckErrors(await _roleManager.UpdateAsync(role));

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.Permissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return MapToEntityDto(role);
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            if (role.IsStatic)
            {
                throw new UserFriendlyException("CannotDeleteAStaticRole");
            }

            var users = await GetUsersInRoleAsync(role.Name);

            foreach (var user in users)
            {
                CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.Name));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        private Task<List<User>> GetUsersInRoleAsync(string roleName)
        {
            var users = (from user in _userRepository.GetAll()
                         join userRole in _userRoleRepository.GetAll() on user.Id equals userRole.UserId
                         join role in _roleRepository.GetAll() on userRole.RoleId equals role.Id
                         where role.Name == roleName
                         select user).Distinct().ToList();

            return Task.FromResult(users);
        }


        protected override IQueryable<Role> CreateFilteredQuery(PagedResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Permissions);
        }

        protected override Task<Role> GetEntityByIdAsync(int id)
        {
            var role = Repository.GetAllIncluding(x => x.Permissions).FirstOrDefault(x => x.Id == id);
            return Task.FromResult(role);
        }

        protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedResultRequestDto input)
        {
            return query.OrderBy(r => r.DisplayName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }


        /////////////////////////////////////////Custom Methods///////////////////////////////////////////

        public async Task<DataTableOutputDto<RoleDto>> GetPaged(GetRolesInput input)
        {
            try
            {


                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            Role role = await _roleRepository.GetAsync(Convert.ToInt32(input.ids[i]));
                            if (role != null)
                            {
                                if (input.action == "Delete")//Delete
                                    await _roleRepository.DeleteAsync(role);
                                else
                                {
                                    role.DeleterUserId = null;
                                    role.DeletionTime = null;
                                    role.IsDeleted = false;
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            Role role = await _roleRepository.GetAsync(Convert.ToInt32(input.ids[0]));
                            if (role != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    if (role.IsStatic)
                                    {
                                        throw new UserFriendlyException("CannotDeleteAStaticRole");
                                    }
                                    await _roleRepository.DeleteAsync(role);
                                }
                                else
                                    role.UnDelete();
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }

                    int count = await _roleRepository.CountAsync();

                    var query = _roleRepository.GetAll();
                    query = query.FilterDataTable(input);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DisplayName), at => at.DisplayName.Contains(input.DisplayName));

                    int filteredCount = await query.CountAsync();

                    var roles =
                            //await query.Include(q => q.CreatorUser.Role).Include(q => q.LastModifierUser.Role).Include(q => q.DeleterUser.Role)
                            await query.OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                            .ToListAsync();

                    return new DataTableOutputDto<RoleDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<RoleDto>>(roles)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /////////////////////////////////////////Permissions Center///////////////////////////////////////////

        public Task<ListResultDto<PermissionDto>> GetAllPermissions()
        {
            var permissions = PermissionManager.GetAllPermissions();

            return Task.FromResult(new ListResultDto<PermissionDto>(
                ObjectMapper.Map<List<PermissionDto>>(permissions)
            ));
        }
        public IEnumerable<UserPermissionDto> GetAllParentPermissions()
        {
            try
            {
                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent == null);
                return ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //////////Role Permissions//////////////
        public async Task<GetRoleWithPermissionsOutput> GetRoleWithPermissionsById(RolePermissionsInput input)
        {
            try
            {
                var role = await _roleRepository.GetAsync(input.RoleId);
                var grantedPermissions = await _roleManager.GetGrantedPermissionsAsync(input.RoleId);
                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);
                return new GetRoleWithPermissionsOutput
                {
                    Role = ObjectMapper.Map<RoleDto>(role),
                    GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task SaveRolePermissions(SaveRolePermissionsInput input)
        {
            foreach (var permission in input.Permissions)
            {
                if (permission.EntityAction == EntityAction.Create)
                    await _roleManager.GrantPermissionAsync(await _roleManager.GetRoleByIdAsync(input.RoleId), PermissionManager.GetPermission(permission.Name));
                else if (permission.EntityAction == EntityAction.Delete)
                    await _roleManager.ProhibitPermissionAsync(await _roleManager.GetRoleByIdAsync(input.RoleId), PermissionManager.GetPermission(permission.Name));
            }
        }
        public async Task<GetRoleWithPermissionsOutput> ClearRoleExplicitPermissions(RolePermissionsInput input)
        {
            try
            {
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                await _roleManager.ResetAllPermissionsAsync(role);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                var grantedPermissions = await _roleManager.GetGrantedPermissionsAsync(role);
                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);
                return new GetRoleWithPermissionsOutput
                {
                    Role = ObjectMapper.Map<RoleDto>(role),
                    GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //////////User Permissions//////////////
        public async Task<GetUserWithPermissionsOutput> GetUserWithPermissionsById(UserPermissionsInput input)
        {
            try
            {

                var user = new User();

                if (input.UserId > 0)
                    user = await _userManager.FindByIdAsync(input.UserId.ToString());

                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);


                if (user != null)
                {
                    var grantedPermissions = await _userManager.GetGrantedPermissionsAsync(user);

                    return new GetUserWithPermissionsOutput
                    {
                        User = ObjectMapper.Map<UserDto>(user),
                        GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                        AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                    };
                }

                return new GetUserWithPermissionsOutput
                {
                    User = ObjectMapper.Map<UserDto>(user),
                    //GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<GetMainEmployeeWithPermissionsOutput> GetMainProviderEmployeePermissions(UserPermissionsInput input)
        {
            try
            {


                var mainProviderUser = await _userRepository.FirstOrDefaultAsync(a => a.MainProviderId == input.MainProviderId.Value && a.UserType == UserTypes.MainProvider);

                var user = new User();

                if (input.UserId > 0)
                    user = await _userManager.FindByIdAsync(input.UserId.ToString());

                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);

                var mainProviderGrantedPermissions = await _userManager.GetGrantedPermissionsAsync(mainProviderUser);

                if (user != null)
                {
                    var grantedPermissions = await _userManager.GetGrantedPermissionsAsync(user);

                    return new GetMainEmployeeWithPermissionsOutput
                    {
                        User = ObjectMapper.Map<UserDto>(user),
                        GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                        MainGrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(mainProviderGrantedPermissions),
                        AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                    };
                }

                return new GetMainEmployeeWithPermissionsOutput
                {
                    User = ObjectMapper.Map<UserDto>(user),
                    MainGrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(mainProviderGrantedPermissions),
                    //GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //public async Task<GetUserWithPermissionsOutput> GetPermissionsById(UserPermissionsInput input)
        //{
        //    try
        //    {
        //        var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
        //        allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);
        //        return new GetUserWithPermissionsOutput
        //        {
        //            User = new UserDto(),
        //            AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}



        public async Task SaveUserPermissions(SaveUsersPermissionsInput input)
        {
            foreach (var permission in input.Permissions)
            {
                if (permission.EntityAction == EntityAction.Create)
                    await _userManager.GrantPermissionAsync(await _userManager.FindByIdAsync(input.UserId.ToString()), PermissionManager.GetPermission(permission.Name));
                else if (permission.EntityAction == EntityAction.Delete)
                    await _userManager.ProhibitPermissionAsync(await _userManager.FindByIdAsync(input.UserId.ToString()), PermissionManager.GetPermission(permission.Name));
            }
        }

        public async Task SaveMainProviderUserPermissions(SaveUsersPermissionsInput input)
        {
            foreach (var permission in input.Permissions)
            {
                if (permission.EntityAction == EntityAction.Create)
                    await _userManager.GrantPermissionAsync(await _userManager.FindByIdAsync(input.UserId.ToString()), PermissionManager.GetPermission(permission.Name));
                else if (permission.EntityAction == EntityAction.Delete)
                {
                    var mainProviderUser = await _userManager.FindByIdAsync(input.UserId.ToString());

                    await _userManager.ProhibitPermissionAsync(mainProviderUser, PermissionManager.GetPermission(permission.Name));


                    //employees
                    var _allEmployees = await _userRepository.GetAll()
                        .Where(a => a.MainProviderId == mainProviderUser.MainProviderId && a.UserType == UserTypes.Employee)
                        .ToListAsync();

                    if (_allEmployees != null)
                    {
                        foreach (var item in _allEmployees.ToList())
                        {
                            bool isGranted = await _userManager.IsGrantedAsync(item.Id, permission.Name);
                            if (isGranted == true)
                                await _userManager.ProhibitPermissionAsync(item, PermissionManager.GetPermission(permission.Name));
                        }
                    }

                    // branches
                    var _branches = await _userRepository.GetAll()
                        .Where(a => a.MainProviderId == mainProviderUser.MainProviderId && a.UserType == UserTypes.Provider)
                        .ToListAsync();

                    if (_branches != null)
                    {
                        string branchPermissionName = permission.Name.Replace("Main", "");


                        var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                        allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == "ProviderPermissions");

                        var existPermission = allPermissions.FirstOrDefault(a => a.Name == branchPermissionName);
                        if (existPermission != null)
                        {
                            foreach (var item in _branches.ToList())
                            {
                                bool isGranted = await _userManager.IsGrantedAsync(item.Id, branchPermissionName);

                                if (isGranted == true)
                                    await _userManager.ProhibitPermissionAsync(item, PermissionManager.GetPermission(branchPermissionName));
                            }
                        }
                    }
                }
            }
        }



        public async Task<GetMainEmployeeWithPermissionsOutput> GetCompanyEmployeePermissions(UserPermissionsInput input)
        {
            try
            {


                var companyUser = await _userRepository.FirstOrDefaultAsync(a => a.CompanyId == input.CompanyId.Value && a.UserType == UserTypes.Company);

                var user = new User();

                if (input.UserId > 0)
                    user = await _userManager.FindByIdAsync(input.UserId.ToString());

                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);

                var companyGrantedPermissions = await _userManager.GetGrantedPermissionsAsync(companyUser);

                if (user != null)
                {
                    var grantedPermissions = await _userManager.GetGrantedPermissionsAsync(user);

                    return new GetMainEmployeeWithPermissionsOutput
                    {
                        User = ObjectMapper.Map<UserDto>(user),
                        GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                        MainGrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(companyGrantedPermissions),
                        AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                    };
                }

                return new GetMainEmployeeWithPermissionsOutput
                {
                    User = ObjectMapper.Map<UserDto>(user),
                    MainGrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(companyGrantedPermissions),
                    //GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task SaveCompanyUserPermissions(SaveUsersPermissionsInput input)
        {
            foreach (var permission in input.Permissions)
            {
                if (permission.EntityAction == EntityAction.Create)
                    await _userManager.GrantPermissionAsync(await _userManager.FindByIdAsync(input.UserId.ToString()), PermissionManager.GetPermission(permission.Name));
                else if (permission.EntityAction == EntityAction.Delete)
                {
                    var companyUser = await _userManager.FindByIdAsync(input.UserId.ToString());

                    await _userManager.ProhibitPermissionAsync(companyUser, PermissionManager.GetPermission(permission.Name));


                    //employees
                    var _allEmployees = await _userRepository.GetAll()
                        .Where(a => a.CompanyId == companyUser.CompanyId && a.UserType == UserTypes.Employee)
                        .ToListAsync();

                    if (_allEmployees != null)
                    {
                        foreach (var item in _allEmployees.ToList())
                        {
                            bool isGranted = await _userManager.IsGrantedAsync(item.Id, permission.Name);
                            if (isGranted == true)
                                await _userManager.ProhibitPermissionAsync(item, PermissionManager.GetPermission(permission.Name));
                        }
                    }

                    // branches
                    var _branches = await _userRepository.GetAll()
                        .Where(a => a.CompanyId == companyUser.CompanyId && a.UserType == UserTypes.Branch)
                        .ToListAsync();

                    if (_branches != null)
                    {
                        string branchPermissionName = permission.Name.Replace("Company", "");


                        var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                        allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == "BranchPermissions");

                        var existPermission = allPermissions.FirstOrDefault(a => a.Name == branchPermissionName);
                        if (existPermission != null)
                        {
                            foreach (var item in _branches.ToList())
                            {
                                bool isGranted = await _userManager.IsGrantedAsync(item.Id, branchPermissionName);

                                if (isGranted == true)
                                    await _userManager.ProhibitPermissionAsync(item, PermissionManager.GetPermission(branchPermissionName));
                            }
                        }
                    }
                }
            }
        }


        public async Task<bool> CreatePermissions(CreatePermissionsInput input)
        {
            // give all permissions to this provider 

            var _allPermissions = await GetUserWithPermissionsById(new UserPermissionsInput { ParentName = input.ParentName });

            var savePermissionsInput = new SaveUsersPermissionsInput
            {
                UserId = input.Id,
                Permissions = new List<CustomPermissionDto>()
            };

            foreach (var permission in _allPermissions.AllPermissions.ToList())
            {
                savePermissionsInput.Permissions.Add(new CustomPermissionDto
                {
                    Name = permission.Name,
                    EntityAction = EntityAction.Create
                });
            }

            await SaveUserPermissions(savePermissionsInput);

            return true;
        }





        public async Task<GetUserWithPermissionsOutput> ClearUserExplicitPermissions(UserPermissionsInput input)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == input.UserId);
                await _userManager.ResetAllPermissionsAsync(user);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                var grantedPermissions = await _userManager.GetGrantedPermissionsAsync(user);
                var allPermissions = PermissionManager.GetAllPermissions().Where(x => x.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide));
                allPermissions = allPermissions.Where(x => x.Parent != null && x.Parent.Name == input.ParentName);
                return new GetUserWithPermissionsOutput
                {
                    User = ObjectMapper.Map<UserDto>(user),
                    GrantedPermissions = ObjectMapper.Map<IReadOnlyList<UserPermissionDto>>(grantedPermissions),
                    AllPermissions = ObjectMapper.Map<IEnumerable<UserPermissionDto>>(allPermissions)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}