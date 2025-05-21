using Abp;
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
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Roles;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System.Globalization;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Companies
{
    public class BranchAppService : AsyncCrudAppService<Branch, BranchDto, long, GetBranchesInput, CreateBranchDto, UpdateBranchDto>, IBranchAppService
    {

        private readonly IUserAppService _userService;
        private readonly IRoleAppService _roleAppService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserDashboard, long> _userDashboardRepository;
        private readonly ICommonAppService _commonService;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly CultureInfo _defaultCulture = new("ar");
        public BranchAppService(
            IRepository<Branch, long> repository,
            IRoleAppService roleAppService,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<UserDashboard, long> userDashboardRepository,
             ICommonAppService commonService,
             AbpNotificationHelper abpNotificationHelper,
                IRepository<UserDevice, long> userDevicesRepository,
                IRepository<Veichle, long> veichleRepository,
            RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _roleAppService = roleAppService;
            _userManager = userManager;
            _userRepository = userRepository;
            _userDashboardRepository = userDashboardRepository;
            _commonService = commonService;
            _userDevicesRepository = userDevicesRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _roleManager = roleManager;
            _veichleRepository = veichleRepository;
        }

        public override async Task<BranchDto> GetAsync(EntityDto<long> input)
        {
            var Branch = await Repository.GetAll()
                .Include(x => x.Company)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Branch);
        }

        [AbpAuthorize]
        public override async Task<BranchDto> CreateAsync(CreateBranchDto input)
        {
            var user = new UserDto();

            if (input.User != null)
            {
                input.User.Name ??= input.NameAr;
                input.User.Surname ??= input.NameAr;
                input.User.UserName ??= input.User.EmailAddress;
                input.User.UserType = UserTypes.Branch;
                user = await _userService.CreateNewUser(input.User);
            }

            if (user.Id <= 0)
            {
                throw new UserFriendlyException(L("Pages.Branches.Error.CantCreateUser"));
            }

            input.UserId = user.Id;
            var branch = ObjectMapper.Map<Branch>(input);
            branch.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Branches", CodeField = "Code" });
            branch = await Repository.InsertAsync(branch);
            await CurrentUnitOfWork.SaveChangesAsync();

            var dbUser = await _userRepository.FirstOrDefaultAsync(user.Id);
            dbUser.BranchId = branch.Id;
            dbUser.CompanyId = branch.CompanyId;
            await _userRepository.UpdateAsync(dbUser);
            await CurrentUnitOfWork.SaveChangesAsync();

            if (input.IsEmployee == true)
            {
                await _userDashboardRepository.InsertAsync(new UserDashboard
                {
                    BranchId = branch.Id,
                    UserId = AbpSession.UserId
                });
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var employees = await _userRepository.GetAll()
                .Where(x => x.AllBranches && x.CompanyId == input.CompanyId)
                .ToListAsync();

            if (input.IsEmployee == true)
            {
                employees = employees.Where(x => x.Id != AbpSession.UserId).ToList();
            }

            foreach (var emp in employees)
            {
                await _userDashboardRepository.InsertAsync(new UserDashboard
                {
                    BranchId = branch.Id,
                    UserId = emp.Id
                });
            }

            await _roleAppService.CreatePermissions(new CreatePermissionsInput { Id = user.Id, ParentName = "BranchPermissions" });

            return MapToEntityDto(branch);
        }

        [AbpAuthorize]
        public override async Task<BranchDto> UpdateAsync(UpdateBranchDto input)
        {
            var branch = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (input.User != null)
            {
                branch.User.Name = string.IsNullOrEmpty(input.User.Name) ? input.NameAr : input.User.Name;
                branch.User.UserName = input.User.UserName;
                branch.User.Surname = string.IsNullOrEmpty(input.User.Surname) ? input.NameAr : input.User.Surname;
                branch.User.PhoneNumber = input.User.PhoneNumber;
                branch.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password == "***************"
                    ? branch.User.Password
                    : _userManager.PasswordHasher.HashPassword(branch.User, input.User.Password);

                branch.User.EmailAddress = input.User.EmailAddress;

                if (!string.IsNullOrEmpty(branch.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == branch.User.UserName && x.Id != branch.UserId && !x.IsDeleted);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }

                if (!string.IsNullOrEmpty(branch.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == branch.User.EmailAddress && x.Id != branch.UserId && !x.IsDeleted);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }

                await _userManager.UpdateAsync(branch.User);
            }

            ObjectMapper.Map(input, branch);
            await Repository.UpdateAsync(branch);
            return MapToEntityDto(branch);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var branch = await Repository.GetAsync(input.Id) ?? throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(branch);
        }
        [AbpAuthorize]
        public override async Task<PagedResultDto<BranchDto>> GetAllAsync(GetBranchesInput input)
        {
            var baseQuery = Repository.GetAll().Include(at => at.User).AsQueryable();

            baseQuery = baseQuery.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name))
                                 .WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive)
                                 .WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId)
                                 .WhereIf(input.CurrentBranchId.HasValue, a => a.Id != input.CurrentBranchId);

            if (input.EmployeeId.HasValue && input.EmployeeId > 0)
            {
                if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                {
                    baseQuery = baseQuery.Where(a => input.BranchesIds.Contains(a.Id));
                }
                else
                {
                    return new PagedResultDto<BranchDto>(0, new List<BranchDto>());
                }
            }

            int count = await baseQuery.CountAsync();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = count;
            }

            var branches = await baseQuery.OrderByDescending(x => x.CreationTime)
                                          .Skip(input.SkipCount)
                                          .Take(input.MaxResultCount)
                                          .ToListAsync();

            var result = ObjectMapper.Map<List<BranchDto>>(branches);
            return new PagedResultDto<BranchDto>(count, result);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<BranchDto>> GetPaged(GetBranchesPagedInput input)
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

                            Branch branch = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (branch != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _branchClinicRepository.CountAsync(a => a.BranchId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Branches.HasClinics"));

                                    await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });

                                    branch.User.IsActive = false;

                                    branch.User.IsDeleted = true;
                                    branch.User.DeletionTime = DateTime.Now;
                                    branch.User.DeleterUserId = AbpSession.UserId;
                                    branch.IsDeleted = true;
                                    branch.DeletionTime = DateTime.Now;
                                    branch.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(branch);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    branch.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(branch);
                                }
                                else if (input.action == "Deactive")
                                {
                                    await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });

                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                    await Repository.UpdateAsync(branch);
                                }

                                else if (input.action == "ManageActive")
                                {

                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    if (branch.User.IsActive == true)
                                    {
                                        await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });
                                        user.IsActive = false;
                                    }
                                    else
                                    {
                                        user.IsActive = true;
                                    }

                                    await _userManager.UpdateAsync(user);
                                }

                                else if (input.action == "Restore")
                                {
                                    branch.UnDelete();
                                    branch.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Branch;

                                    branch.User.Roles = [];
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        branch.User.Roles.Add(new UserRole(AbpSession.TenantId, branch.UserId.Value, role.Id));

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

                            Branch branch = await Repository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
                            if (branch != null)
                            {
                                if (input.action == "Delete")
                                {
                                    await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });

                                    // check before delete 
                                    //int clinicsCount = await _branchClinicRepository.CountAsync(a => a.BranchId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Branches.HasClinics"));
                                    branch.User.IsActive = false;
                                    branch.User.IsDeleted = true;
                                    branch.User.DeletionTime = DateTime.Now;
                                    branch.User.DeleterUserId = AbpSession.UserId;
                                    branch.IsDeleted = true;
                                    branch.DeletionTime = DateTime.Now;
                                    branch.DeleterUserId = AbpSession.UserId;

                                    await Repository.UpdateAsync(branch);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                //else if (input.action == "Restore")
                                //    branch.UnDelete();
                                else if (input.action == "Active")
                                {
                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    user.IsActive = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                }
                                else if (input.action == "Deactive")
                                {
                                    await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });

                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    user.IsActive = false;
                                    await _userManager.UpdateAsync(user);
                                }

                                else if (input.action == "ManageActive")
                                {

                                    var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                                    if (branch.User.IsActive == true)
                                    {
                                        await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });
                                        user.IsActive = false;
                                    }
                                    else
                                    {
                                        user.IsActive = true;
                                    }

                                    await _userManager.UpdateAsync(user);
                                }

                                else if (input.action == "Restore")
                                {
                                    branch.UnDelete();
                                    branch.User.UnDelete();
                                    // check if user role is deleted 

                                    string roleName = RolesNames.Branch;

                                    branch.User.Roles = [];
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        branch.User.Roles.Add(new UserRole(AbpSession.TenantId, branch.UserId.Value, role.Id));
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    if (input.CompanyId.HasValue == false)
                        return new DataTableOutputDto<BranchDto>
                        {
                            iTotalDisplayRecords = 0,
                            iTotalRecords = 0,
                            aaData = []
                        };

                    var query = Repository.GetAll().Include(a => a.User).Where(at => at.User.UserType == UserTypes.Branch);

                    query = query.WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId);


                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.Id));
                        }
                        else
                            return new DataTableOutputDto<BranchDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = []
                            };
                    }


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ManagerName), at => at.User.Name.Contains(input.ManagerName) || at.User.Surname.Contains(input.ManagerName));
                    query = query.WhereIf(input.IsActive.HasValue, a => a.User.IsActive == input.IsActive);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    query = query.WhereIf(input.BranchId.HasValue, a => a.Id == input.BranchId);

                    query = query.WhereIf(input.FuelAmountFrom.HasValue, at => at.FuelAmount >= input.FuelAmountFrom);
                    query = query.WhereIf(input.FuelAmountTo.HasValue, at => at.FuelAmount <= input.FuelAmountTo);

                    query = query.WhereIf(input.CleanAmountFrom.HasValue, at => at.CleanAmount >= input.CleanAmountFrom);
                    query = query.WhereIf(input.CleanAmountTo.HasValue, at => at.CleanAmount <= input.CleanAmountTo);

                    query = query.WhereIf(input.MaintainAmountFrom.HasValue, at => at.MaintainAmount >= input.MaintainAmountFrom);
                    query = query.WhereIf(input.MaintainAmountTo.HasValue, at => at.MaintainAmount <= input.MaintainAmountTo);

                    query = query.WhereIf(input.ConsumptionFrom.HasValue, at => at.ConsumptionAmount >= input.ConsumptionFrom);
                    query = query.WhereIf(input.ConsumptionTo.HasValue, at => at.ConsumptionAmount <= input.ConsumptionTo);

                    query = query.WhereIf(input.VeichlesFrom.HasValue, at => at.VeichlesCount >= input.VeichlesFrom);
                    query = query.WhereIf(input.VeichlesTo.HasValue, at => at.VeichlesCount <= input.VeichlesTo);


                    int filteredCount = await query.CountAsync();
                    var branches = await query.Include(x => x.User)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser).OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    var _branches = ObjectMapper.Map<List<BranchDto>>(branches);
                    foreach (var item in _branches.ToList())
                    {
                        item.ActVeichlesCount = await _veichleRepository.CountAsync(x => x.BranchId == item.Id && x.IsDeleted == false);
                    }

                    return new DataTableOutputDto<BranchDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = _branches
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [AbpAuthorize]
        public async Task<ManageActiveOutput> ManageActive(EntityDto<long> input)
        {
            // get logged in user 
            var _loggedInUser = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
            var branch = await Repository.FirstOrDefaultAsync(input.Id);

            if (branch.CompanyId == _loggedInUser.CompanyId)
            {
                var user = await _userManager.FindByIdAsync(branch.UserId.Value.ToString());
                if (branch.User.IsActive == true)
                {
                    await SendLogoutNotification(new EntityDto<long> { Id = branch.Id });
                    user.IsActive = false;
                }
                else
                {
                    user.IsActive = true;
                }

                await _userManager.UpdateAsync(user);

                return new ManageActiveOutput { IsActive = user.IsActive, UserId = branch.UserId.Value };
            }
            else return new ManageActiveOutput { };
        }

        public async Task<BranchDto> GetByUserId(EntityDto<long> input)
        {
            var Branch = await Repository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.UserId == input.Id);
            return MapToEntityDto(Branch);
        }

        public async Task SendLogoutNotification(EntityDto<long> input)
        {
            var users = await _userRepository.GetAll().Where(a => a.BranchId == input.Id).ToListAsync();
            if (users.Count > 0)
            {

                foreach (var user in users)
                {
                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                    var _lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, user.Id));
                    _lang ??= _defaultCulture.ToString();

                    FCMPushNotification fcmPushClient = new FCMPushNotification();
                    var userDevices = await _userDevicesRepository.GetAll().Where(x => x.UserId == user.Id).ToListAsync();

                    if (userDevices != null && userDevices.Count > 0)
                    {
                        foreach (var item in userDevices)
                        {
                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                            {
                                RegistrationToken = item.RegistrationToken,
                                Title = L("Common.SystemTitle"),
                                Message = L("Pages.Notifications.LogOut"),
                                Type = FcmNotificationType.Logout

                            });
                        }
                    }

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: user.Id));
                    CreateNotificationDto _createUserNotificationData = new CreateNotificationDto
                    {
                        //SenderUserName = (await _userManager.GetUserByIdAsync(AbpSession.UserId.Value)).Name,
                        Message = L("Pages.Notifications.LogOut"),
                        EntityType = Entity_Type.Logout,
                        //EntityId = input.AdvertisementId,
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Logout, _createUserNotificationData, targetUsersId.ToArray());
                }
            }
        }
        public async Task<GetBranchWalletDetailsDto> GetWalletDetails(GetBranchesInput input)
        {
            var client = Repository.GetAll().FirstOrDefault(x => x.Id == input.Id && x.CompanyId == input.CompanyId);
            return await Task.FromResult(ObjectMapper.Map<GetBranchWalletDetailsDto>(client));
        }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        [AbpAuthorize]
        public async Task<BranchDto> UpdateReservedAndBalance(UpdateReservedBalanceBranchDto input)
        {
            await _semaphore.WaitAsync();
            try
            {
                var branch = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (input.OperationType == OperationType.Begin)
                {
                    branch.Reserved = branch.Reserved + input.Reserved;
                }
                else if (input.OperationType == OperationType.End)
                {
                    branch.Reserved = branch.Reserved - input.Reserved;
                    branch.FuelAmount = branch.FuelAmount - input.Price;
                    branch.WalletAmount = branch.WalletAmount - input.Price;
                }
                else
                {
                    branch.Reserved = branch.Reserved - input.Reserved;
                }
                await Repository.UpdateAsync(branch);
                return MapToEntityDto(branch);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [AbpAuthorize]
        public async Task<List<BranchNameDto>> GetAllBranchs(GetBranchesInput input)
        {
            var query = Repository.GetAll().Include(at => at.User).AsQueryable();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(input.IsActive.HasValue, at => at.User.IsActive == input.IsActive);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.CompanyId == input.CompanyId);

            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var branches = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<BranchNameDto>>(branches);
            return _mappedList;
        }
    }
}
