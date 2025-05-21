using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.UI;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Contact.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.SendingMails;
using Sayarah.Application.Roles.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Configuration;
using Sayarah.Core.Helpers;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Users
{

    public class UserAppService : AsyncCrudAppService<User, UserDto, long, GetAllUsersInput, CreateUserDto, UpdateUserDto>, IUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserDevice, long> _userDeviceRepository;
        private readonly IRepository<UserDashboard, long> _userDashboardRepository;
        private readonly ICommonAppService _commonService;
        private readonly ISettingManager _settingManager;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly ICommonAppService _commonAppService;
        private readonly ISendingMailsAppService _sendingMailsAppService;
        CultureInfo new_lang = new CultureInfo("ar");
        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            IRepository<Role> roleRepository,
            RoleManager roleManager,
            IRepository<UserDevice, long> userDeviceRepository,
             IRepository<UserDashboard, long> userDashboardRepository,
             ICommonAppService commonService,
             ISettingManager settingManager,
              AbpNotificationHelper abpNotificationHelper,
              ICommonAppService commonAppService,
              ISendingMailsAppService sendingMailsAppService
          ) : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userManager = userManager;
            _roleRepository = roleRepository;
            _roleManager = roleManager;
            _userDeviceRepository = userDeviceRepository;
            _userDashboardRepository = userDashboardRepository;
            _commonService = commonService;
            _settingManager = settingManager;
            _abpNotificationHelper = abpNotificationHelper;
            _commonAppService = commonAppService;
            _sendingMailsAppService = sendingMailsAppService;
        }

        //FCMPushNotification fcmPushClient = new FCMPushNotification();
        public override async Task<UserDto> GetAsync(EntityDto<long> input)
        {
            var existingUser = _userManager.GetUserById(input.Id);

            var user = ObjectMapper.Map<UserDto>(await Repository.GetAllIncluding(x => x.Roles).Include(a => a.UserDashboards.Select(aa => aa.Branch)).Include(a => a.UserDashboards.Select(aa => aa.Provider)).FirstOrDefaultAsync(x => x.Id == input.Id));
            var userRoles = await _userManager.GetRolesAsync(existingUser);

            return user;
        }

        public async Task<string> GetAvatarPath(EntityDto<long> input)
        {


            var _user = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            string _avatarPath = FilesPath.Users.DefaultImagePath;

            if (_user != null)
            {
                if (!string.IsNullOrEmpty(_user.Avatar) && (_user.Avatar.Contains("https://platform-lookaside.fbsbx.com/") || _user.Avatar.Contains("https://lh3.googleusercontent.com/")))
                {
                    _avatarPath = _user.Avatar;
                }

                if (!string.IsNullOrEmpty(_user.Avatar) && Utilities.CheckExistImage(1, "400x400_" + _user.Avatar))
                {
                    _avatarPath = FilesPath.Users.ServerImagePath + "400x400_" + _user.Avatar;
                }
            }

            return _avatarPath;
        }
        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "AbpUsers", CodeField = "Code" });

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.Password = new PasswordHasher().HashPassword(input.Password);
            user.IsEmailConfirmed = true;

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.RoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            }

            CheckErrors(await _userManager.CreateAsync(user));

            await CurrentUnitOfWork.SaveChangesAsync();

            return MapToEntityDto(user);
        }
        public override async Task<UserDto> UpdateAsync(UpdateUserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            return await GetAsync(input);
        }
        [AbpAuthorize]
        public async Task<DataTableOutputDto<UserDto>> GetPaged(GetPagedInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    long id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        foreach (var strId in input.ids)
                        {
                            id = Convert.ToInt64(strId);
                            var user = await Repository.FirstOrDefaultAsync(id);
                            if (user != null)
                            {
                                switch (input.action)
                                {
                                    case "Delete":
                                        user.IsDeleted = true;
                                        user.DeleterUserId = AbpSession.UserId;
                                        user.DeletionTime = DateTime.UtcNow;
                                        await Repository.UpdateAsync(user);
                                        break;
                                    case "Restore":
                                        user.UnDelete();
                                        string roleName = RolesNames.Employee;
                                        if (user.UserType == UserTypes.Admin)
                                            roleName = RolesNames.Admin;
                                        else if (user.UserType == UserTypes.Employee)
                                            roleName = RolesNames.Employee;
                                        else if (user.UserType == UserTypes.Client)
                                            roleName = RolesNames.Client;
                                        user.Roles = new List<UserRole>();
                                        var role = await _roleManager.GetRoleByNameAsync(roleName);
                                        if (role != null)
                                            user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
                                        break;
                                    case "Active":
                                        user.IsActive = true;
                                        user.IsEmailConfirmed = true;
                                        user.LockoutEndDateUtc = null;
                                        await _userManager.UpdateAsync(user);
                                        await UnitOfWorkManager.Current.SaveChangesAsync();
                                        break;
                                    case "Deactive":
                                        user.IsActive = false;
                                        user.IsEmailConfirmed = false;
                                        break;
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction" && input.ids.Length > 0)
                    {
                        id = Convert.ToInt64(input.ids[0]);
                        var user = await Repository.FirstOrDefaultAsync(id);
                        if (user != null)
                        {
                            switch (input.action)
                            {
                                case "Delete":
                                    user.IsDeleted = true;
                                    user.DeleterUserId = AbpSession.UserId;
                                    user.DeletionTime = DateTime.UtcNow;
                                    await Repository.UpdateAsync(user);
                                    break;
                                case "Restore":
                                    user.UnDelete();
                                    string roleName = RolesNames.Employee;
                                    if (user.UserType == UserTypes.Admin)
                                        roleName = RolesNames.Admin;
                                    else if (user.UserType == UserTypes.Employee)
                                        roleName = RolesNames.Employee;
                                    else if (user.UserType == UserTypes.Client)
                                        roleName = RolesNames.Client;
                                    user.Roles = new List<UserRole>();
                                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                                    if (role != null)
                                        user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
                                    break;
                                case "Active":
                                    user.IsActive = true;
                                    user.IsEmailConfirmed = true;
                                    user.LockoutEndDateUtc = null;
                                    await _userManager.UpdateAsync(user);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();
                                    break;
                                case "Deactive":
                                    user.IsActive = false;
                                    user.IsEmailConfirmed = false;
                                    break;
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    var query = Repository.GetAll().Include(x => x.Roles).AsQueryable();

                    query = query.WhereIf(input.UserType.HasValue, a => a.UserType == input.UserType);
                    query = query.WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.BranchId.HasValue, a => a.BranchId == input.BranchId);
                    query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.MainEmployees.HasValue, a => !a.BranchId.HasValue);
                    query = query.WhereIf(input.CurrentEmployeeId.HasValue, a => a.Id != input.CurrentEmployeeId);

                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Surname), at => at.Surname.Contains(input.Surname));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.UserName.Contains(input.UserName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.EmailAddress), at => at.EmailAddress.Contains(input.EmailAddress));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.PhoneNumber.Contains(input.PhoneNumber));
                    query = query.WhereIf(input.UserType.HasValue, a => a.UserType == input.UserType);
                    query = query.WhereIf(input.IsActive.HasValue, a => a.IsActive == input.IsActive);

                    int filteredCount = await query.CountAsync();
                    var users = await query
                        .Include(x => x.CreatorUser)
                        .Include(x => x.LastModifierUser)
                        .Include(x => x.DeleterUser)
                        .ToListAsync();

                    var usersList = ObjectMapper.Map<List<UserDto>>(users);

                    if (input.IsSpecialId.HasValue)
                    {
                        usersList.ForEach(m => { if (m.Id == input.IsSpecialId.Value) m.IsSpecial = true; });
                        usersList = usersList.OrderByDescending(x => x.IsSpecial).Skip(input.start).Take(input.length).ToList();
                    }
                    else
                    {
                        usersList =await usersList.AsQueryable()
                            .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                            .ToListAsync();
                    }

                    return new DataTableOutputDto<UserDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = usersList
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<UserDto> CreateNewUser(CreateNewUserInput input)
        {
            try
            {
                CheckCreatePermission();

                if (!string.IsNullOrEmpty(input.UserName))
                {
                    var existUser = await Repository.FirstOrDefaultAsync(x => x.UserName == input.UserName && x.IsDeleted == false);
                    if (existUser != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }

                Random random = new Random();
                string random_text = LocalizationSourceName + random.Next(100000000).ToString();

                if (!string.IsNullOrEmpty(input.EmailAddress))
                {
                    var existEmail = await Repository.FirstOrDefaultAsync(x => x.EmailAddress == input.EmailAddress && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }
                else
                {
                    input.EmailAddress = random_text + "@Sayarah.com";
                }

                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var existUserPhone = await Repository.FirstOrDefaultAsync(x => x.PhoneNumber == input.PhoneNumber && !string.IsNullOrEmpty(x.PhoneNumber) && x.IsDeleted == false);
                    if (existUserPhone != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistPhone"));
                }
                else
                {
                    input.PhoneNumber = random_text;
                }
                input.UserName = string.IsNullOrEmpty(input.UserName) ? random_text : input.UserName;
                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "AbpUsers", CodeField = "Code" });
                var user = ObjectMapper.Map<User>(input);

                if (string.IsNullOrEmpty(input.Name))
                    user.Name = random_text;
                if (string.IsNullOrEmpty(input.Surname))
                    user.Surname = input.Name;

                //
                user.Password = new PasswordHasher().HashPassword(input.Password);
                user.IsEmailConfirmed = true;
                //
                if (!string.IsNullOrEmpty(input.Avatar))
                    user.Avatar = input.Avatar;

                //Assign roles
                if (!string.IsNullOrEmpty(input.RoleName))
                {
                    user.Roles = new List<UserRole>();
                    var role = await _roleManager.GetRoleByNameAsync(input.RoleName);
                    if (role != null)
                        user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
                }
                //user.UserName = user.Name;
                CheckErrors(await _userManager.CreateAsync(user));
                // await Repository.InsertAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();



                if (input.UserDashboards != null)
                {
                    foreach (var item in input.UserDashboards.ToList())
                    {
                        item.UserId = user.Id;

                        switch (item.EntityAction)
                        {
                            case EntityAction.Create:
                            case EntityAction.Update:
                                var dashboard = ObjectMapper.Map<UserDashboard>(item);
                                await _userDashboardRepository.InsertAsync(dashboard);
                                break;
                            case EntityAction.Delete:
                                if (item.Id > 0)
                                {
                                    await _userDashboardRepository.DeleteAsync(item.Id.Value);
                                }
                                break;
                        }
                    }

                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }

                return MapToEntityDto(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<UserDto> UpdateUser(CreateNewUserInput input)
        {
            try
            {
                CheckUpdatePermission();

                var user = await _userManager.GetUserByIdAsync(input.Id);

                Random random = new Random();
                string random_text = LocalizationSourceName + random.Next(100000000).ToString();

                if (!string.IsNullOrEmpty(input.EmailAddress))
                {
                    var existEmail = await Repository.FirstOrDefaultAsync(x => x.EmailAddress == input.EmailAddress && x.Id != input.Id && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }
                else
                {
                    input.EmailAddress = random_text + "@Sayarah.com";
                }

                if (!string.IsNullOrEmpty(input.PhoneNumber))
                {
                    var existUserPhone = await Repository.FirstOrDefaultAsync(x => x.PhoneNumber == input.PhoneNumber && x.Id != input.Id && !string.IsNullOrEmpty(x.PhoneNumber) && x.IsDeleted == false);
                    if (existUserPhone != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }
                else
                {
                    input.PhoneNumber = random_text;
                }

                if (string.IsNullOrEmpty(input.UserName))
                    input.UserName = random_text;
                else
                {
                    var existUserName = await Repository.FirstOrDefaultAsync(x => x.UserName == input.UserName && x.Id != input.Id && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }

                input.Password = string.IsNullOrEmpty(input.Password) || input.Password.Equals("***************") ?
                user.Password : _userManager.PasswordHasher.HashPassword(user, input.Password);

                ObjectMapper.Map(input, user);

                CheckErrors(await _userManager.UpdateAsync(user));
                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.UserDashboards != null)
                {
                    foreach (var item in input.UserDashboards.ToList())
                    {
                        item.UserId = user.Id;

                        switch (item.EntityAction)
                        {
                            case EntityAction.Create:
                            case EntityAction.Update:
                                var dashboard = ObjectMapper.Map<UserDashboard>(item);
                                await _userDashboardRepository.InsertAsync(dashboard);
                                break;
                            case EntityAction.Delete:
                                if (item.Id > 0)
                                {
                                    await _userDashboardRepository.DeleteAsync(item.Id.Value);
                                }
                                break;
                        }
                    }

                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }


                return await GetAsync(new EntityDto<long> { Id = user.Id });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }
        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }
        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            return user;
        }
        protected override void MapToEntity(UpdateUserDto input, User user)
        {
            ObjectMapper.Map(input, user);
        }
        protected override IQueryable<User> CreateFilteredQuery(GetAllUsersInput input)
        {
            return Repository.GetAllIncluding(x => x.Roles);
        }
        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = Repository.GetAllIncluding(x => x.Roles).FirstOrDefault(x => x.Id == id);
            return await Task.FromResult(user);
        }
        protected override IQueryable<User> ApplySorting(IQueryable<User> query, GetAllUsersInput input)
        {
            return query.OrderBy(r => r.UserName);
        }
        protected virtual void CheckErrors(Microsoft.AspNetCore.Identity.IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
        [AbpAuthorize]
        public async Task<ChangeUserPasswordOutput> ChangeUserPassword(UpdateUserInput input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);
                var result = _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
                if (result.IsCompletedSuccessfully == true)
                {
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return new ChangeUserPasswordOutput { Message = L("Pages.Users.Error.PasswordChanged"), Success = true };
                }
                else
                    return new ChangeUserPasswordOutput { Message = L("Pages.Users.Error.PasswordInCorrect"), Success = false }; ;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<User>> GetFilteredForRegister(GetPagedInput input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var query = Repository.GetAll();

                query = query.WhereIf(!string.IsNullOrEmpty(input.EmailAddress), at => at.EmailAddress.Equals(input.EmailAddress));
                query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.UserName.Equals(input.UserName));
                query = query.WhereIf(!string.IsNullOrEmpty(input.PhoneNumber), at => at.PhoneNumber.Equals(input.PhoneNumber));

                return ObjectMapper.Map<List<User>>(await query.ToListAsync());
            }
        }
        public async Task DeleteForRegister(long id)
        {
            await _commonService.ExecuteSqlAsync(await _settingManager.GetSettingValueAsync(AppSettingNames.WebApiKey), await _settingManager.GetSettingValueAsync(AppSettingNames.AddApiKey), "Delete from AbpUsers where id=" + id);
        }
        [AbpAuthorize]
        public async Task<UserDto> UpdateUserApi(CreateNewUserInput input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);

                var existEmailAddress = await Repository.GetAll().Where(x => x.EmailAddress == input.EmailAddress && x.Id != AbpSession.UserId.Value).FirstOrDefaultAsync();
                if (existEmailAddress != null)
                    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));

                //var existEPhoneNumber = await Repository.GetAll().Where(x => x.PhoneNumber == input.PhoneNumber && x.Id != AbpSession.UserId.Value).FirstOrDefaultAsync();
                //if (existEPhoneNumber != null)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistPhone"));

                var existUsername = await Repository.GetAll().Where(x => x.UserName == input.UserName && x.Id != AbpSession.UserId.Value).FirstOrDefaultAsync();
                if (existUsername != null)
                    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                user.Password = string.IsNullOrEmpty(input.Password) || input.Password.Equals("***************") ?
                  user.Password : _userManager.PasswordHasher.HashPassword(user, input.Password);

                user.Name = !string.IsNullOrEmpty(input.Name) ? input.Name : user.Name;
                user.Surname = !string.IsNullOrEmpty(input.Surname) ? input.Surname : user.Surname;
                user.EmailAddress = !string.IsNullOrEmpty(input.EmailAddress) ? input.EmailAddress : user.EmailAddress;
                user.PhoneNumber = !string.IsNullOrEmpty(input.PhoneNumber) ? input.PhoneNumber : user.PhoneNumber;


                if (!string.IsNullOrEmpty(input.Avatar))
                    user.Avatar = input.Avatar;

                CheckErrors(await _userManager.UpdateAsync(user));
                await CurrentUnitOfWork.SaveChangesAsync();

                return await GetAsync(new EntityDto<long> { Id = user.Id });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<User>> GetUsersByRole(EntityDto<int> input)
        {
            try
            {
                var admins = await _userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == input.Id)).ToListAsync();
                return admins;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override async Task<PagedResultDto<UserDto>> GetAllAsync(GetAllUsersInput input)
        {
            var query = Repository.GetAll();
            query = query.WhereIf(input.UserType.HasValue, at => at.UserType == input.UserType.Value);
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.Name.Contains(input.Name));

            if (input.MaxCount)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }
            int count = query.Count();
            var users = await query.OrderByDescending(m => m.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

            return new PagedResultDto<UserDto>(count, ObjectMapper.Map<List<UserDto>>(users));
        }
        /////////////////////////////////////////UserDevices///////////////////////////////////////////
        public async Task<string> UserManageDevices(ManageUserDevicesInput input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input.RegistrationToken))
                {
                    var userDevice = await _userDeviceRepository.GetAll().Where(x => x.DeviceType == input.DeviceType && x.RegistrationToken == input.RegistrationToken).FirstOrDefaultAsync();
                    if ((userDevice == null || userDevice.Id == 0) && input.UserId.HasValue)
                    {
                        userDevice = new UserDevice
                        {
                            DeviceType = input.DeviceType,
                            RegistrationToken = input.RegistrationToken,
                            UserId = input.UserId.Value
                        };
                        userDevice = await _userDeviceRepository.InsertAsync(userDevice);
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else
                    {
                        userDevice.UserId = input.UserId;
                        userDevice = await _userDeviceRepository.UpdateAsync(userDevice);
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                }
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<long> UserGetByDevice(GetUserByDeviceInput input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input.RegistrationToken))
                {
                    var userDevice = await _userDeviceRepository.GetAll().Include(x => x.User).Where(x => x.DeviceType == input.DeviceType && x.RegistrationToken == input.RegistrationToken).FirstOrDefaultAsync();
                    if (userDevice != null && userDevice.Id > 0 && userDevice.User != null)
                    {
                        return userDevice.UserId.Value;
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public async Task<List<UserDevice>> GetAllUserDevices(GetAllUserDevicesInput input)
        {
            try
            {
                var query = _userDeviceRepository.GetAll();
                query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.User.Name.Equals(input.UserName));
                var userDevices = await query.Include(x => x.User).ToListAsync();

                return userDevices;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<List<UserPlainDto>> GetAllUsersHaveDevices(GetAllUserDevicesInput input)
        {
            try
            {
                var query = Repository.GetAll().Include(x => x.UserDevices).AsQueryable()   ;
                query = query.Where(x => x.UserDevices.Count > 0);
                query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), at => at.Name.Equals(input.UserName));
                var users = await query.ToListAsync();
                return ObjectMapper.Map<List<UserPlainDto>>(users);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<DeactivateAccountOutput> DeactivateAccount(EntityDto<long> input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);
                user.IsActive = false;
                user.IsEmailConfirmed = false;
                user.IsPhoneNumberConfirmed = false;
                user.IsDeleted = true;
                CheckErrors(await _userManager.UpdateAsync(user));
                await CurrentUnitOfWork.SaveChangesAsync();

                var userDevices = await _userDeviceRepository.GetAll().Where(x => x.UserId == user.Id).ToListAsync();
                if (userDevices != null && userDevices.Count > 0)
                {
                    FCMPushNotification fcmPushClient = new FCMPushNotification();

                    foreach (var item in userDevices)
                    {
                        var _lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, item.UserId.Value));
                        if (_lang == null)
                            _lang = new_lang.ToString();
                        FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                        {
                            RegistrationToken = item.RegistrationToken,
                            Title = L("Common.SystemTitle", new CultureInfo(_lang)),
                            Body = L("Pages.Notifications.DeactiveClient", new CultureInfo(_lang)),
                            Type = FcmNotificationType.Deactive,
                            PatternId = user.Id,
                        });

                        await _userDeviceRepository.DeleteAsync(item);
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();

                return new DeactivateAccountOutput { Success = true };


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ///////////////////////////////////////SendNotifications///////////////////////////////////////////
        [AbpAuthorize]
        public async Task SendNotifications(SendNotificationsInput input)
        {
            FCMPushNotification fcmPushClient = new FCMPushNotification();

            if (input.UsersList != null && input.UsersList.Count > 0)
            {
                List<UserIdentifier> targetAdminUsersId = new List<UserIdentifier>();

                if (input.NotificationType == NotificationType.Mobile || input.NotificationType == NotificationType.Both)
                {
                    foreach (var user in input.UsersList.ToList())
                    {
                        // get  user email address and set it in ids list 


                        var userDevices = await _userDeviceRepository.GetAllListAsync(x => x.UserId == user.Id);
                        foreach (var userDevice in userDevices)
                        {
                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                            {
                                RegistrationToken = userDevice.RegistrationToken,
                                Title = L("Common.SystemTitle"),
                                Body = input.NotificationMessage,
                                Type = FcmNotificationType.Public
                            });
                        }

                        targetAdminUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: user.Id));
                    }

                }

                if (input.NotificationType == NotificationType.Mobile || input.NotificationType == NotificationType.Both)
                {
                    var notiProperties = new Dictionary<string, object>();
                    notiProperties.Add("MessageAr", input.NotificationMessage);
                    notiProperties.Add("MessageEn", input.NotificationMessage);

                    CreateNotificationDto _createNotificationData = new CreateNotificationDto
                    {
                        SenderUserName = "Common.SystemTitle",
                        Message = input.NotificationMessage,
                        EntityType = Entity_Type.Public,
                        Properties = notiProperties
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Public, _createNotificationData, targetAdminUsersId.ToArray());
                }


                if (input.NotificationType == NotificationType.Email || input.NotificationType == NotificationType.Both)
                {

                    List<string> _listEmails = new List<string>();
                    foreach (var e in input.UsersList.ToList())
                    {
                        _listEmails.Add(e.EmailAddress);
                    }

                    string[] Emails = _listEmails.ToArray();

                    // send email here 
                    var Result = await _commonAppService.SendEmail(new SendEmailRequest
                    {
                        Emails = Emails,
                        // Url = callbackUrl,
                        //UrlTitle = L("Pages.Register.Login"),
                        datalst = new[] { input.NotificationMessage }
                    });
                }


            }
        }


        [AbpAuthorize]
        public async Task<bool> SendNotificationToUsers(SendNotificationsInput input)
        {

            FCMPushNotification fcmPushClient = new FCMPushNotification();
            var userDevices = _userDeviceRepository.GetAll().Where(at => at.UserId != AbpSession.UserId.Value);
            userDevices = userDevices.WhereIf(input.NotificationFilter.HasValue && input.NotificationFilter == NotificationFilter.Administrative, x => x.User.UserType == UserTypes.Admin);
            var UserDevices = await userDevices.ToListAsync();
            if (UserDevices != null && UserDevices.Count > 0)
            {
                foreach (var userDevice in userDevices)
                {
                    FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                    {
                        RegistrationToken = userDevice.RegistrationToken,
                        Title = L("Common.SystemTitle"),
                        Body = input.NotificationMessage,
                        Type = FcmNotificationType.Public
                    });
                }
                return true;
            }
            else
            {
                return false;
            }


        }
        [AbpAuthorize]
        public async Task SendNotificationsToAll(SendNotificationsInput input)
        {

            if (input.NotificationType == NotificationType.Mobile || input.NotificationType == NotificationType.Both)
            {

                FCMPushNotification fcmPushClient = new FCMPushNotification();
                FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotificationToAllDevices(new FcmNotificationInput()
                {
                    Title = L("Common.SystemTitle"),
                    Body = input.NotificationMessage,
                    Type = FcmNotificationType.Public
                });

            }


            // get All users and save to abp notification
            var _allUsers = await Repository.GetAll().Where(a => a.UserType == UserTypes.Client).ToListAsync();
            List<UserIdentifier> targetAdminUsersId = new List<UserIdentifier>();

            List<string> _listEmails = new List<string>();
            foreach (var item in _allUsers.ToList())
            {

                if (input.NotificationType == NotificationType.Mobile || input.NotificationType == NotificationType.Both)
                {
                    targetAdminUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: item.Id));
                }

                if (input.NotificationType == NotificationType.Email || input.NotificationType == NotificationType.Both)
                {
                    _listEmails.Add(item.EmailAddress);
                }
            }


            if (input.NotificationType == NotificationType.Mobile || input.NotificationType == NotificationType.Both)
            {

                var notiProperties = new Dictionary<string, object>();

                notiProperties.Add("MessageAr", input.NotificationMessage);
                notiProperties.Add("MessageEn", input.NotificationMessage);

                CreateNotificationDto _createNotificationData = new CreateNotificationDto
                {
                    SenderUserName = "Common.SystemTitle",
                    Message = input.NotificationMessage,
                    EntityType = Entity_Type.Public,
                    Properties = notiProperties
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Public, _createNotificationData, targetAdminUsersId.ToArray());
            }


            if (input.NotificationType == NotificationType.Email || input.NotificationType == NotificationType.Both)
            {
                string[] Emails = _listEmails.ToArray();
                // send email here 
                var Result = await _commonAppService.SendEmail(new SendEmailRequest
                {
                    Emails = Emails,
                    // Url = callbackUrl,
                    //UrlTitle = L("Pages.Register.Login"),
                    datalst = new[] { input.NotificationMessage }
                });
            }

        }
        public async Task<UserDto> GetById(EntityDto<long> input)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == input.Id);
            var users = ObjectMapper.Map<UserDto>(user);
            return users;
        }
        public async Task<UserDto> GetUserById(EntityDto<long> input)
        {
            var user = await Repository.GetAll().FirstOrDefaultAsync(x => x.Id == input.Id);
            var _user = ObjectMapper.Map<UserDto>(user);
            return _user;
        }
        public async Task<User> GetUserByPhone(GetUserByPhone input)
        {
            var user = Repository.FirstOrDefault(x => x.PhoneNumber == input.PhoneNumber);
            return await Task.FromResult(user);
        }
        async Task<GetSettingDataDto> GetSettings()
        {
            return new GetSettingDataDto()
            {
                Host = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host),
                Password = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password),
                Port = await SettingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port),
                Sender = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName),
            };
        }

        [AbpAuthorize]
        [DisableAuditing]
        public async Task<UpdateMainColorDto> UpdateMainColor(UpdateMainColorDto input)
        {

            var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            user.MainColor = input.MainColor;
            await Repository.UpdateAsync(user);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            return input;
        }

        [AbpAuthorize]
        [DisableAuditing]
        public async Task<UpdateDarkModeDto> UpdateDarkMode(UpdateDarkModeDto input)
        {

            var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            user.DarkMode = input.DarkMode;
            await Repository.UpdateAsync(user);
            await UnitOfWorkManager.Current.SaveChangesAsync();
            return input;
        }





        public async Task<SendEmailCodeOutput> HandleEmailAddress(SendEmailCodeInput input)
        {
            try
            {


                // check if phone exists 
                var existUser = await Repository.FirstOrDefaultAsync(a => a.EmailAddress == input.EmailAddress && a.IsDeleted == false);
                if (existUser == null)
                    throw new UserFriendlyException(L("Pages.Users.Error.NotExistUserEmail"));

                existUser.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
                await Repository.UpdateAsync(existUser);


                //List<string> emails = new List<string>();
                //emails.Add(input.EmailAddress);

                //string[] ownerEmails = emails.ToArray();


                //var ownerResult = await _sendingMailsAppService.SendEmail(new SendEmailRequest
                //{
                //    Emails = ownerEmails,
                //    datalst = new[] { L("MobileApi.Messages.CodeMessage" ,existUser.EmailConfirmationCode )
                //    }
                //});

                return new SendEmailCodeOutput { Id = existUser.Id, EmailAddress = input.EmailAddress, /*EmailConfirmationCode = existUser.EmailConfirmationCode ,*/ Success = true };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<HandleConfirmEmailAddressOutput> HandleConfirmEmailAddress(HandleConfirmEmailAddressInput input)
        {
            try
            {

                if (input.Id > 0)
                {
                    var user = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

                    if (user.EmailConfirmationCode == input.EmailConfirmationCode)
                    {
                        // set phone is confirmed true
                        user.IsEmailConfirmed = true;
                        await Repository.UpdateAsync(user);
                        return new HandleConfirmEmailAddressOutput { Id = user.Id, EmailAddress = user.EmailAddress, Success = true };
                    }
                    else
                    {
                        throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                    }
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<HandleConfirmEmailAddressOutput> CreatePassword(HandleConfirmEmailAddressInput input)
        {
            try
            {

                if (input.Id > 0)
                {
                    var user = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);

                    user.Password = new PasswordHasher().HashPassword(input.Password);
                    //Save user
                    //user.IsEmailConfirmed = true;
                    //user.IsActive = true;
                    user.IsPhoneNumberConfirmed = true;
                    user.EmailConfirmationCode = null;

                    CheckErrors(await _userManager.UpdateAsync(user));
                    await UnitOfWorkManager.Current.SaveChangesAsync();

                    return new HandleConfirmEmailAddressOutput { Success = true };

                    //if (user.EmailConfirmationCode == input.EmailConfirmationCode)
                    //{
                    //    // set phone is confirmed true
                    //    user.IsEmailConfirmed = true;
                    //    await Repository.UpdateAsync(user);
                    //    return new HandleConfirmEmailAddressOutput { Id = user.Id, EmailAddress = user.EmailAddress, Success = true };
                    //}
                    //else
                    //{
                    //    throw new UserFriendlyException(L("MobileApi.Messages.ConfirmationCodeError"));
                    //}
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.RegisterationRequests.ErrorSending"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> UpdatePassword(UpdatePasswordInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {

                    var user = await _userManager.GetUserByIdAsync(input.Id);

                    user.Password = string.IsNullOrEmpty(input.Password) || input.Password.Equals("***************") ?
                      user.Password : _userManager.PasswordHasher.HashPassword(user, input.Password);

                    await _userManager.UpdateAsync(user);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}