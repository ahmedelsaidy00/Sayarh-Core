using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Sayarah.Application.Helpers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Roles;
using Sayarah.Application.Users;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Providers
{
    public class ProviderAppService : AsyncCrudAppService<Provider, ProviderDto, long, GetProvidersInput, CreateProviderDto, UpdateProviderDto>, IProviderAppService
    {

        private readonly IUserAppService _userService;
        private readonly IRoleAppService _roleAppService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserDashboard, long> _userDashboardRepository;
        private readonly ICommonAppService _commonService;
        private readonly RoleManager _roleManager;
        public ProviderAppService(
            IRepository<Provider, long> repository,
            IUserAppService userService,
            IRoleAppService roleAppService,
            UserManager userManager,
            ICommonAppService commonService,
            IRepository<User, long> userRepository,
            IRepository<UserDashboard, long> userDashboardRepository,
            RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _roleAppService = roleAppService;
            _userManager = userManager;
            _commonService = commonService;
            _userRepository = userRepository;
            _userDashboardRepository = userDashboardRepository;
            _roleManager = roleManager;
        }

        public override async Task<ProviderDto> GetAsync(EntityDto<long> input)
        {
            var Provider = await Repository.GetAll()
                .Include(x => x.User)
                .Include(x => x.MainProvider)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Provider);
        }

        [AbpAuthorize]
        public override async Task<ProviderDto> CreateAsync(CreateProviderDto input)
        {
            try
            {
                var user = new UserDto();
                if (input.User != null)
                {
                    input.User.Name = string.IsNullOrEmpty(input.User.Name) ? input.NameAr : input.User.Name;
                    input.User.Surname = string.IsNullOrEmpty(input.User.Surname) ? input.NameAr : input.User.Surname;
                    input.User.PhoneNumber = input.PhoneNumber;
                    input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                    input.User.UserType = UserTypes.Provider;
                    user = await _userService.CreateNewUser(input.User);
                }
                if (user.Id > 0)
                {
                    input.UserId = user.Id;
                    var provider = ObjectMapper.Map<Provider>(input);
                    provider.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Providers", CodeField = "Code" });
                    provider.VisibleInMap = true;
                    provider = await Repository.InsertAsync(provider);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var _user = await _userRepository.FirstOrDefaultAsync(user.Id);
                    _user.ProviderId = provider.Id;
                    _user.MainProviderId = input.MainProviderId;
                    await _userRepository.UpdateAsync(_user);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {
                        await _userDashboardRepository.InsertAsync(new UserDashboard
                        {
                            ProviderId = provider.Id,
                            UserId = AbpSession.UserId
                        });
                    }

                    var _emps = await _userRepository.GetAll()
                        .Where(a => a.AllBranches == true && a.MainProviderId.HasValue == true && a.MainProviderId == input.MainProviderId)
                        .ToListAsync();

                    if (_emps != null)
                    {
                        _emps = _emps.WhereIf(input.IsEmployee.HasValue && input.IsEmployee == true, a => a.Id != AbpSession.UserId).ToList();
                        foreach (var emp in _emps)
                        {
                            await _userDashboardRepository.InsertAsync(new UserDashboard
                            {
                                ProviderId = provider.Id,
                                UserId = emp.Id
                            });
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    await _roleAppService.CreatePermissions(new CreatePermissionsInput { Id = user.Id, ParentName = "ProviderPermissions" });
                    return MapToEntityDto(provider);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Providers.Error.CantCreateUser"));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [AbpAuthorize]
        public override async Task<ProviderDto> UpdateAsync(UpdateProviderDto input)
        {
            var provider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(provider.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !provider.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(6, provider.Avatar, new string[] { "600x600_" });

            if (input.User != null)
            {
                provider.User.Name = string.IsNullOrEmpty(input.User.Name) ? input.NameAr : input.User.Name;
                provider.User.UserName = input.User.UserName;
                provider.User.Surname = string.IsNullOrEmpty(input.User.Surname) ? input.NameAr : input.User.Surname;
                provider.User.PhoneNumber = input.PhoneNumber;
                input.User.PhoneNumber = input.PhoneNumber;

                provider.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                    provider.User.Password : _userManager.PasswordHasher.HashPassword(provider.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                    provider.User.Password : _userManager.PasswordHasher.HashPassword(provider.User, input.User.Password);

                provider.User.EmailAddress = string.IsNullOrEmpty(input.EmailAddress) ? input.User.EmailAddress : input.EmailAddress;

                if (!string.IsNullOrEmpty(provider.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == provider.User.UserName && x.Id != provider.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }

                if (!string.IsNullOrEmpty(provider.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == provider.User.EmailAddress && x.Id != provider.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }

                await _userManager.UpdateAsync(provider.User);
            }
            ObjectMapper.Map(input, provider);
            await Repository.UpdateAsync(provider);
            return MapToEntityDto(provider);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var provider = await Repository.FirstOrDefaultAsync(input.Id) ?? throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(provider);
        }

        public override async Task<PagedResultDto<ProviderDto>> GetAllAsync(GetProvidersInput input)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var query = Repository.GetAll().Include(at => at.User).AsQueryable();
                query = query.WhereIf(input.ShowDeleted == false, a => a.IsDeleted == false);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                query = query.WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);
                query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);

                if (input.EmployeeId.HasValue && input.EmployeeId > 0)
                {
                    List<long> branchesIds = new List<long>();
                    var users = await _userDashboardRepository.GetAll().Where(a => a.UserId == input.EmployeeId && a.ProviderId.HasValue).ToListAsync();
                    if (users != null)
                    {
                        foreach (var item in users)
                        {
                            branchesIds.Add(item.ProviderId.Value);
                        }
                    }

                    if (branchesIds != null && branchesIds.Count > 0)
                    {
                        query = query.Where(a => branchesIds.Contains(a.Id));
                    }
                    else
                        return new PagedResultDto<ProviderDto>(0, new List<ProviderDto>());
                }

                int count = await query.CountAsync();
                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = count;
                }

                var providers = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                var _mappedList = ObjectMapper.Map<List<ProviderDto>>(providers);
                return new PagedResultDto<ProviderDto>(count, _mappedList);
            }
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<ProviderDto>> GetPaged(GetProvidersPagedInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    int id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            id = Convert.ToInt32(input.ids[i]);
                            Provider provider = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (provider != null)
                            {
                                if (input.action == "Delete")
                                {
                                    provider.User.IsDeleted = true;
                                    provider.User.DeletionTime = DateTime.Now;
                                    provider.User.DeleterUserId = AbpSession.UserId;

                                    provider.IsDeleted = true;
                                    provider.DeletionTime = DateTime.Now;
                                    provider.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(provider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();
                                }
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(provider.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(provider);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(provider.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(provider);
                                }
                                else if (input.action == "Restore")
                                {
                                    provider.UnDelete();
                                    provider.User.UnDelete();

                                    string roleName = RolesNames.Provider;
                                    provider.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        provider.User.Roles.Add(new UserRole(AbpSession.TenantId, provider.UserId.Value, role.Id));
                                }
                                else if (input.action == "VisibleInMap")
                                {
                                    provider.VisibleInMap = !provider.VisibleInMap;
                                    await Repository.UpdateAsync(provider);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            Provider provider = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (provider != null)
                            {
                                if (input.action == "Delete")
                                {
                                    provider.User.IsDeleted = true;
                                    provider.User.DeletionTime = DateTime.Now;
                                    provider.User.DeleterUserId = AbpSession.UserId;

                                    provider.IsDeleted = true;
                                    provider.DeletionTime = DateTime.Now;
                                    provider.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(provider);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();
                                }
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(provider.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Deactive")
                                {
                                    var user = await _userManager.FindByIdAsync(provider.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Restore")
                                {
                                    provider.UnDelete();
                                    provider.User.UnDelete();

                                    string roleName = RolesNames.Provider;
                                    provider.User.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        provider.User.Roles.Add(new UserRole(AbpSession.TenantId, provider.UserId.Value, role.Id));
                                }
                                else if (input.action == "VisibleInMap")
                                {
                                    provider.VisibleInMap = !provider.VisibleInMap;
                                    await Repository.UpdateAsync(provider);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(a => a.User)
                        .Where(at => at.User.UserType == UserTypes.Provider);

                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);

                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {
                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.Where(a => input.BranchesIds.Contains(a.Id));
                        }
                        else
                            return new DataTableOutputDto<ProviderDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<ProviderDto>()
                            };
                    }

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ManagerName), at => (at.User.Name + " " + at.User.Surname).Contains(input.ManagerName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.User.UserName.Contains(input.UserName));
                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    query = query.WhereIf(input.ProviderId.HasValue, a => a.Id == input.ProviderId);
                    int filteredCount = await query.CountAsync();
                    var providers = await query.Include(x => x.User)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<ProviderDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<ProviderDto>>(providers)
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ProviderDto> GetByUserId(EntityDto<long> input)
        {
            var Provider = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(Provider);
        }
        public async Task<PagedResultDto<ApiProviderDto>> GetAllProvidersByMainProviderIdMobile(GetProvidersByMainProviderIdInputApi input)
        {
            try
            {

                var query = Repository.GetAll().Include(a => a.MainProvider).Where(a => a.User.IsActive == true);
                query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                query = query.WhereIf(input.IsFuel.HasValue, a => a.IsFuel == input.IsFuel);
                query = query.WhereIf(input.IsOil.HasValue, a => a.IsOil == input.IsOil);
                query = query.WhereIf(input.IsClean.HasValue, a => a.IsClean == input.IsClean);
                query = query.WhereIf(input.IsMaintain.HasValue, a => a.IsMaintain == input.IsMaintain);
                query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.VisibleInMap.HasValue, a => a.VisibleInMap == input.VisibleInMap);


                if (input.Latitude.HasValue && input.Latitude.Value > 0 && input.Longitude.HasValue && input.Longitude.Value > 0)
                {
                    // Replace the selected code block with the following .NET Core compatible version
                    query = query
                        .Where(provider => provider.Latitude.HasValue && provider.Longitude.HasValue && input.Latitude.HasValue && input.Longitude.HasValue)
                        .OrderBy(provider =>
                            12742 * Math.Asin(Math.Sqrt(
                                Math.Sin(((Math.PI / 180) * (provider.Latitude.Value - input.Latitude.Value)) / 2) * Math.Sin(((Math.PI / 180) * (provider.Latitude.Value - input.Latitude.Value)) / 2) +
                                Math.Cos((Math.PI / 180) * input.Latitude.Value) * Math.Cos((Math.PI / 180) * provider.Latitude.Value) *
                                Math.Sin(((Math.PI / 180) * (provider.Longitude.Value - input.Longitude.Value)) / 2) * Math.Sin(((Math.PI / 180) * (provider.Longitude.Value - input.Longitude.Value)) / 2)
                            )))
                        .Concat(query.Where(provider => !provider.Latitude.HasValue || !provider.Longitude.HasValue));
                }
                else
                {
                    query = query.OrderBy(x => x.CreationTime);
                }
                int count = query.Count();
                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var providers = await query/*.OrderByDescending(x => x.CreationTime)*/.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                var _mappedList = ObjectMapper.Map<List<ApiProviderDto>>(providers);
                if (input.CalculateDistance.HasValue && input.CalculateDistance == true)
                {
                    foreach (var item in _mappedList)
                    {
                        double distance = 0;
                        if (item.Latitude.HasValue && item.Longitude.HasValue && input.Latitude.HasValue && input.Longitude.HasValue)
                            distance = GeoCoordinateHepler.GetDistance(item.Latitude.Value, item.Longitude.Value, input.Latitude.Value, input.Longitude.Value);
                        item.Distance = distance;
                    }
                }
                return new PagedResultDto<ApiProviderDto>(count, _mappedList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<PagedResultDto<ApiProviderDto>> GetAllProvidersMobile(GetProvidersInputApi input)
        {
            try
            {
                var query = Repository.GetAll()
                    .Include(a => a.MainProvider)
                    .Include(a => a.User)
                    .Where(a => a.User.IsActive == true);

                query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                query = query.WhereIf(input.IsFuel.HasValue, a => a.IsFuel == input.IsFuel);
                query = query.WhereIf(input.IsOil.HasValue, a => a.IsOil == input.IsOil);
                query = query.WhereIf(input.IsClean.HasValue, a => a.IsClean == input.IsClean);
                query = query.WhereIf(input.IsMaintain.HasValue, a => a.IsMaintain == input.IsMaintain);
                query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                query = query.WhereIf(input.VisibleInMap.HasValue, a => a.VisibleInMap == input.VisibleInMap);

                // Calculate distance if coordinates are provided
                bool hasCoordinates = input.Latitude.HasValue && input.Latitude.Value > 0 && input.Longitude.HasValue && input.Longitude.Value > 0;
                if (hasCoordinates)
                {
                    double lat = input.Latitude.Value;
                    double lng = input.Longitude.Value;
                    query = query.OrderBy(a =>
                        (a.Latitude.HasValue && a.Longitude.HasValue)
                            ? (Math.Pow(a.Latitude.Value - lat, 2) + Math.Pow(a.Longitude.Value - lng, 2))
                            : double.MaxValue
                    );
                }
                else
                {
                    query = query.OrderBy(x => x.CreationTime);
                }

                int count = await query.CountAsync();
                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = count;
                }

                var providers = await query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();

                var _mappedList = ObjectMapper.Map<List<ApiProviderDto>>(providers);

                if (input.CalculateDistance.HasValue && input.CalculateDistance == true && hasCoordinates)
                {
                    foreach (var item in _mappedList)
                    {
                        double distance = 0;
                        if (item.Latitude.HasValue && item.Longitude.HasValue)
                        {
                            // Haversine formula
                            double R = 6371; // Radius of earth in km
                            double dLat = (item.Latitude.Value - input.Latitude.Value) * Math.PI / 180.0;
                            double dLon = (item.Longitude.Value - input.Longitude.Value) * Math.PI / 180.0;
                            double lat1 = input.Latitude.Value * Math.PI / 180.0;
                            double lat2 = item.Latitude.Value * Math.PI / 180.0;

                            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                            distance = R * c;
                        }
                        item.Distance = distance;
                    }
                }

                return new PagedResultDto<ApiProviderDto>(count, _mappedList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<PagedResultDto<PlainProviderDto>> GetAllLocations(GetProvidersInput input)
        {
            // Build query with includes and filters
            var query = Repository.GetAll()
                .Include(at => at.User)
                .Include(a => a.MainProvider)
                .Where(a => a.User.IsActive == true);

            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);
            query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
            query = query.WhereIf(input.IsFuel.HasValue, a => a.IsFuel == input.IsFuel);
            query = query.WhereIf(input.IsClean.HasValue, a => a.IsClean == input.IsClean);
            query = query.WhereIf(input.IsOil.HasValue, a => a.IsOil == input.IsOil);
            query = query.WhereIf(input.IsMaintain.HasValue, a => a.IsMaintain == input.IsMaintain);
            query = query.WhereIf(input.VisibleInMap.HasValue, a => a.VisibleInMap == input.VisibleInMap);

            int count = await query.CountAsync();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = count;
            }

            var providers = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var _mappedList = ObjectMapper.Map<List<PlainProviderDto>>(providers);

            return new PagedResultDto<PlainProviderDto>(count, _mappedList);
        }



    }
}
