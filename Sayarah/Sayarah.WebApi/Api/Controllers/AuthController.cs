using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.WebApi.Controllers;
using Sayarah.Api.Models;
using Sayarah.Authorization;
using Sayarah.Authorization.Users;
using Sayarah.Helpers;
using Sayarah.Users;
using Sayarah.Users.Dto;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static Sayarah.SayarahConsts;
using Abp;
using Sayarah.Helpers.Enums;
using Sayarah.Security;
using Abp.Notifications;
using System.Security.Claims;
using Abp.IdentityFramework;
using Abp.Configuration;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using Sayarah.Drivers;
using Sayarah.Workers;
using Sayarah.Providers.Dto;
using Sayarah.Providers;
using Castle.MicroKernel.Registration;
using Sayarah.Companies;
using Sayarah.Configuration;
using static Sayarah.SayarahConsts.FilesPath;
using Sayarah.Companies.Dto;

namespace Sayarah.Api.Controllers
{

    public class AuthController : AbpApiController
    {
        public AppSession AppSession { get; set; }
        private readonly LogInManager _logInManager;
        private readonly ISettingManager _settingManager;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDriverAppService _driverAppService;
        private readonly IWorkerAppService _workerAppService;
        private readonly ITenantCache _tenantCache;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUserAppService _userAppService;
        private readonly IFuelPumpAppService _fuelPumpAppService;
        private readonly ICompanyAppService _companyAppService;
        private readonly IRepository<UserDevice, long> _userDeviceRepository;
        private readonly IBranchAppService _branchAppService;


        private readonly IUserNotificationManager _userNotificationManager;
        public AuthController(
                  LogInManager logInManager,
                  UserManager userManager,
                  ISettingManager settingManager,
                  IRepository<User, long> userRepository,
                  IUserAppService userAppService,
                  IUnitOfWorkManager unitOfWorkManager,
                  ITenantCache tenantCache,
                  IDriverAppService driverAppService,
                  IWorkerAppService workerAppService,
                  IRepository<UserDevice, long> userDeviceRepository,
                  IUserNotificationManager userNotificationManager,

                   ICompanyAppService companyAppService,
                  IFuelPumpAppService fuelPumpAppService,
                  IBranchAppService branchAppService
                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _logInManager = logInManager;
            _userManager = userManager;
            _settingManager = settingManager;
            _unitOfWorkManager = unitOfWorkManager;
            _tenantCache = tenantCache;
            _userRepository = userRepository;
            _userAppService = userAppService;
            _userDeviceRepository = userDeviceRepository;
            _userNotificationManager = userNotificationManager;
            _driverAppService = driverAppService;
            _workerAppService = workerAppService;
            _fuelPumpAppService = fuelPumpAppService;
            _companyAppService = companyAppService;
            _branchAppService = branchAppService;
        }


        CultureInfo new_lang = new CultureInfo("ar");

        #region User Actions  
        [HttpPost]
        [Language("Lang")]
        public async Task<LoginOutput> Login(LoginInput input)
        {
            // get user with the phone number 
            try
            {
               
                var loginResult = await _logInManager.LoginByUsernameOrEmail(input.UserName, input.Password, GetTenancyNameOrNull());

                switch (loginResult.Result)
                {
                    case AbpLoginResultType.InvalidPassword:
                        return new LoginOutput { Message = L("MobileApi.Messages.InvalidPassword") };

                    case AbpLoginResultType.UserIsNotActive:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive") }; // UserIsNotActiveAndCanNotLogin
                    case AbpLoginResultType.UserEmailIsNotConfirmed:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) }; // UserEmailIsNotConfirmedAndCanNotLogin
                    case AbpLoginResultType.LockedOut:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) };
                    case AbpLoginResultType.Success:


                        if (input.UserType.HasValue && input.UserType != loginResult.User.UserType)
                            return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };


                        if (loginResult.User.UserType == UserTypes.Driver || loginResult.User.UserType == UserTypes.Worker || loginResult.User.UserType == UserTypes.AdminEmployee || loginResult.User.UserType == UserTypes.Company)
                        {
                            if (loginResult.User.UserType == UserTypes.Worker)
                            {
                                var AppVersion = "2.1.0";
                                if (input.AppVersion== null || input.AppVersion != AppVersion)
                                {
                                    return new LoginOutput { Message = L("MobileApi.Messages.upgradApp") };
                                }
                            }
                            //var client = await _driverAppService.GetByUserId(new EntityDto<long> { Id = loginResult.User.Id });

                            var ticket = new AuthenticationTicket(await _userManager.CreateIdentityAsync(loginResult.User, DefaultAuthenticationTypes.ApplicationCookie), new AuthenticationProperties());

                            var currentUtc = new SystemClock().UtcNow;
                            ticket.Properties.IssuedUtc = currentUtc;
                            ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(30));

                            List<ApiFuelPumpDto> _fuelPump = new List<ApiFuelPumpDto>();

                            IEnumerable<Claim> rolesList = ticket.Identity.Claims;
                            var roleClaim = rolesList.FirstOrDefault(c => c.Type == ticket.Identity.RoleClaimType);
                            if (roleClaim.Value == RolesNames.Worker || roleClaim.Value == RolesNames.Driver || roleClaim.Value == RolesNames.AdminEmployee || roleClaim.Value == RolesNames.Company)
                            {

                                var userDevicesByUser = await _userDeviceRepository.GetAllListAsync(x => x.UserId == loginResult.User.Id);
                                if (userDevicesByUser != null && userDevicesByUser.Count > 0)
                                {
                                    foreach (var item in userDevicesByUser)
                                    {
                                        if (item.RegistrationToken != input.RegistrationToken)
                                        {
                                            FCMPushNotification fcmPushClient = new FCMPushNotification();
                                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                                            {
                                                RegistrationToken = item.RegistrationToken,
                                                Title = L("Common.SystemTitle"),
                                                Type = FcmNotificationType.Logout,
                                                Body = L("MobileApi.Messages.IsLogedinFromAnthorDevice")
                                            });
                                            await _userDeviceRepository.DeleteAsync(item);
                                        }
                                    }
                                }


                                var userDevice = await _userDeviceRepository.FirstOrDefaultAsync(x => x.RegistrationToken == input.RegistrationToken && x.DeviceType == input.DeviceType);
                                if (userDevice != null)
                                {
                                    userDevice.UserId = loginResult.User.Id;
                                    await _userDeviceRepository.UpdateAsync(userDevice);
                                }
                                else
                                    await _userDeviceRepository.InsertAsync(new UserDevice { UserId = loginResult.User.Id, RegistrationToken = input.RegistrationToken, DeviceType = input.DeviceType });

                                // unread notification count
                                int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: loginResult.User.Id), UserNotificationState.Unread);


                                // Driver Avatar and license

                                string _avatarPath = string.Empty;


                                bool IsFuel = false;
                                bool IsOil = false;
                                bool IsClean = false;
                                bool IsMaintain = false;



                                string code = string.Empty;
                                if (loginResult.User.UserType == UserTypes.Driver)
                                {

                                    if (!string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(6, "600x600_" + loginResult.User.Avatar))
                                        _avatarPath = FilesPath.Drivers.ServerImagePath + "600x600_" + loginResult.User.Avatar;
                                    else
                                        _avatarPath = FilesPath.Drivers.DefaultImagePath;

                                    // get driver 
                                    var driver = await _driverAppService
                                        .GetByUserId(new EntityDto<long> { Id = loginResult.User.Id });

                                    if (driver != null)
                                    {
                                        code = driver.Code;

                                        if (driver.Branch == null || driver.Branch.Company == null)
                                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                                        if (driver.Branch.User.IsActive == false || driver.Branch.Company.User.IsActive == false)
                                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                                    }

                                }
                                else if (loginResult.User.UserType == UserTypes.Worker)
                                {
                                    if (!string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(8, "600x600_" + loginResult.User.Avatar))
                                        _avatarPath = FilesPath.Workers.ServerImagePath + "600x600_" + loginResult.User.Avatar;
                                    else
                                        _avatarPath = FilesPath.Workers.DefaultImagePath;

                                    var worker = await _workerAppService.GetByUserId(new EntityDto<long> { Id = loginResult.User.Id });
                                    if (worker != null)
                                    {
                                        IsFuel = worker.IsFuel;
                                        IsOil = worker.IsOil;
                                        IsClean = worker.IsClean;
                                        IsMaintain = worker.IsMaintain;
                                        code = worker.Code;

                                        if (worker.Provider == null || worker.Provider.MainProvider == null)
                                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                                        if (worker.Provider.User.IsActive == false || worker.Provider.MainProvider.User.IsActive == false)
                                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                                    }

                                    if (loginResult.User.ProviderId.HasValue)
                                    {

                                        // get list of fuel pumps 
                                        var fuelPumps = await _fuelPumpAppService.GetAllAsync(new GetFuelPumpsInput { ProviderId = loginResult.User.ProviderId, MaxCount = true });
                                        _fuelPump = ObjectMapper.Map<List<ApiFuelPumpDto>>(fuelPumps.Items);

                                    }
                                }


                                return new LoginOutput
                                {
                                    PhoneNumber = loginResult.User.PhoneNumber,
                                    AccessToken = AccountController.OAuthBearerOptions.AccessTokenFormat.Protect(ticket),
                                    Name = loginResult.User.Name,
                                    Avatar = _avatarPath,
                                    UserType = loginResult.User.UserType,
                                    UserId = loginResult.User.Id,
                                    EmailAddress = loginResult.User.EmailAddress,
                                    BranchId = loginResult.User.BranchId,
                                    CompanyId = loginResult.User.CompanyId,
                                    ProviderId = loginResult.User.ProviderId,
                                    WorkerId = loginResult.User.WorkerId,
                                    UserName = loginResult.User.EmailAddress,
                                    IsActive = loginResult.User.IsActive,
                                    IsClean = IsClean,
                                    IsMaintain = IsMaintain,
                                    IsOil = IsOil,
                                    IsFuel = IsFuel,
                                    Success = true,
                                    Code = code,
                                    FuelPumps = _fuelPump
                                };
                            }
                            else
                            {
                                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                            }
                        }
                        else
                            return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                }
                return new LoginOutput { Message = L("MobileApi.Messages.InvalidEmailAddress") };

            }
            catch (Exception ex)
            {
                return new LoginOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<LoginOutput> SimAppLogin(LoginInput input)
        {
            // get user with the phone number 
            try
            {
                var loginResult = await _logInManager.LoginByUsernameOrEmail(input.UserName, input.Password, GetTenancyNameOrNull());

                switch (loginResult.Result)
                {
                    case AbpLoginResultType.InvalidPassword:
                        return new LoginOutput { Message = L("MobileApi.Messages.InvalidPassword") };

                    case AbpLoginResultType.UserIsNotActive:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive") }; // UserIsNotActiveAndCanNotLogin
                    case AbpLoginResultType.UserEmailIsNotConfirmed:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) }; // UserEmailIsNotConfirmedAndCanNotLogin
                    case AbpLoginResultType.LockedOut:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) };
                    case AbpLoginResultType.Success:


                        if (loginResult.User.UserType == UserTypes.Admin || loginResult.User.UserType == UserTypes.Company || loginResult.User.UserType == UserTypes.Employee)
                        {

                            if (loginResult.User.UserType == UserTypes.Employee)
                            {
                                if (loginResult.User.CompanyId.HasValue == false)
                                    return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };


                                // get company 
                                var company = await _companyAppService.GetAsync(new EntityDto<long> { Id = loginResult.User.CompanyId.Value });
                                if (company == null)
                                    return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };

                                if (company.IsDeleted == true || company.User.IsActive == false)
                                    return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };

                            }

                            //var client = await _driverAppService.GetByUserId(new EntityDto<long> { Id = loginResult.User.Id });

                            var ticket = new AuthenticationTicket(await _userManager.CreateIdentityAsync(loginResult.User, DefaultAuthenticationTypes.ApplicationCookie), new AuthenticationProperties());

                            var currentUtc = new SystemClock().UtcNow;
                            ticket.Properties.IssuedUtc = currentUtc;
                            ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(30));


                            IEnumerable<Claim> rolesList = ticket.Identity.Claims;
                            var roleClaim = rolesList.FirstOrDefault(c => c.Type == ticket.Identity.RoleClaimType);
                            if (roleClaim.Value == RolesNames.Admin || roleClaim.Value == RolesNames.Company || roleClaim.Value == RolesNames.Employee)
                            {

                                var userDevicesByUser = await _userDeviceRepository.GetAllListAsync(x => x.UserId == loginResult.User.Id);
                                if (userDevicesByUser != null && userDevicesByUser.Count > 0)
                                {
                                    foreach (var item in userDevicesByUser)
                                    {
                                        if (item.RegistrationToken != input.RegistrationToken)
                                        {
                                            FCMPushNotification fcmPushClient = new FCMPushNotification();
                                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                                            {
                                                RegistrationToken = item.RegistrationToken,
                                                Title = L("Common.SystemTitle"),
                                                Type = FcmNotificationType.Logout,
                                                Body = L("MobileApi.Messages.IsLogedinFromAnthorDevice")
                                            });
                                            await _userDeviceRepository.DeleteAsync(item);
                                        }
                                    }
                                }


                                var userDevice = await _userDeviceRepository.FirstOrDefaultAsync(x => x.RegistrationToken == input.RegistrationToken && x.DeviceType == input.DeviceType);
                                if (userDevice != null)
                                {
                                    userDevice.UserId = loginResult.User.Id;
                                    await _userDeviceRepository.UpdateAsync(userDevice);
                                }
                                else
                                    await _userDeviceRepository.InsertAsync(new UserDevice { UserId = loginResult.User.Id, RegistrationToken = input.RegistrationToken, DeviceType = input.DeviceType });

                                // unread notification count
                                int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: loginResult.User.Id), UserNotificationState.Unread);

                                // Driver Avatar and license

                                string _avatarPath = string.Empty;


                                if (loginResult.User.UserType == UserTypes.Admin || loginResult.User.UserType == UserTypes.Employee)
                                {
                                    if (!string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(1, "400x400_" + loginResult.User.Avatar))
                                        _avatarPath = FilesPath.Users.ServerImagePath + "400x400_" + loginResult.User.Avatar;
                                    else
                                        _avatarPath = FilesPath.Users.DefaultImagePath;
                                }
                                else if (loginResult.User.UserType == UserTypes.Company)
                                {
                                    if (!string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(4, "600x600_" + loginResult.User.Avatar))
                                        _avatarPath = FilesPath.Companies.ServerImagePath + "600x600_" + loginResult.User.Avatar;
                                    else
                                        _avatarPath = FilesPath.Companies.DefaultImagePath;
                                }


                                return new LoginOutput
                                {
                                    PhoneNumber = loginResult.User.PhoneNumber,
                                    AccessToken = AccountController.OAuthBearerOptions.AccessTokenFormat.Protect(ticket),
                                    Name = loginResult.User.Name,
                                    Avatar = _avatarPath,
                                    UserType = loginResult.User.UserType,
                                    UserId = loginResult.User.Id,
                                    EmailAddress = loginResult.User.EmailAddress,
                                    BranchId = loginResult.User.BranchId,
                                    CompanyId = loginResult.User.CompanyId,
                                    ProviderId = loginResult.User.ProviderId,
                                    WorkerId = loginResult.User.WorkerId,
                                    UserName = loginResult.User.EmailAddress,
                                    IsActive = loginResult.User.IsActive,
                                    Success = true
                                };
                            }
                            else
                            {
                                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                            }
                        }
                        else
                            return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                }
                return new LoginOutput { Message = L("MobileApi.Messages.InvalidEmailAddress") };

            }
            catch (Exception ex)
            {
                return new LoginOutput { Message = ex.Message };
            }
        }



        [HttpPost]
        [Language("Lang")]
        public async Task<LoginOutput> RefreshAccessToken(RegistrationTokenInput input)
        {
            try
            {
                bool forceUpdate = (await SettingManager.GetSettingValueAsync(AppSettingNames.ForceUpdate)) == "false" ? false : true;

                //string _91color = await SettingManager.GetSettingValueAsync(AppSettingNames._91color);
                //string _95color = await SettingManager.GetSettingValueAsync(AppSettingNames._95color);
                //string _Dieselcolor = await SettingManager.GetSettingValueAsync(AppSettingNames._Dieselcolor);

                 

                var userId = await _userAppService.UserGetByDevice(new GetUserByDeviceInput { DeviceType = input.DeviceType, RegistrationToken = input.RegistrationToken });
                if (userId > 0)
                {
                    var user = await _userManager.GetUserByIdAsync(userId);
                    if(user == null)
                        return new LoginOutput {};

                    var ticket = new AuthenticationTicket(await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie), new AuthenticationProperties());
                    var currentUtc = new SystemClock().UtcNow;
                    ticket.Properties.IssuedUtc = currentUtc;
                    ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(30));



                    var Lang = HttpContext.Current.Request.Headers["Lang"];
                    new_lang = string.IsNullOrEmpty(Lang) ? new_lang : new CultureInfo(Lang);

                    string _avatarPath = string.Empty;



                    bool IsFuel = false;
                    bool IsOil = false;
                    bool IsClean = false;
                    bool IsMaintain = false;
                    string code = string.Empty;

                    List<ApiFuelPumpDto> _fuelPump = new List<ApiFuelPumpDto>();

                    if (user.UserType == UserTypes.Driver)
                    {

                        if (!string.IsNullOrEmpty(user.Avatar) && Utilities.CheckExistImage(6, "600x600_" + user.Avatar))
                            _avatarPath = FilesPath.Drivers.ServerImagePath + "600x600_" + user.Avatar;
                        else
                            _avatarPath = FilesPath.Drivers.DefaultImagePath;

                        // get driver 
                        var driver = await _driverAppService.GetByUserId(new EntityDto<long> { Id = user.Id });
                        if (driver != null)
                        {
                            code = driver.Code;


                            if (driver.Branch == null || driver.Branch.Company == null)
                                return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                            if (driver.Branch.User.IsActive == false || driver.Branch.Company.User.IsActive == false)
                                return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin


                        }
                    }
                    else if (user.UserType == UserTypes.Worker)
                    {
                        if (!string.IsNullOrEmpty(user.Avatar) && Utilities.CheckExistImage(8, "600x600_" + user.Avatar))
                            _avatarPath = FilesPath.Workers.ServerImagePath + "600x600_" + user.Avatar;
                        else
                            _avatarPath = FilesPath.Workers.DefaultImagePath;

                        var worker = await _workerAppService.GetByUserId(new EntityDto<long> { Id = user.Id });
                        if (worker != null)
                        {
                            IsFuel = worker.IsFuel;
                            IsOil = worker.IsOil;
                            IsClean = worker.IsClean;
                            IsMaintain = worker.IsMaintain;
                            code = worker.Code;


                            if (worker.Provider == null || worker.Provider.MainProvider == null)
                                return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                            if (worker.Provider.User.IsActive == false || worker.Provider.MainProvider.User.IsActive == false)
                                return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") }; // UserIsNotActiveAndCanNotLogin

                        }


                        if (user.ProviderId.HasValue)
                        {

                            // get list of fuel pumps 
                            var fuelPumps = await _fuelPumpAppService.GetAllAsync(new GetFuelPumpsInput { ProviderId = user.ProviderId, MaxCount = true });
                            _fuelPump = ObjectMapper.Map<List<ApiFuelPumpDto>>(fuelPumps.Items);

                        }

                    }


                    // unread notification count
                    int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: user.Id), UserNotificationState.Unread);



                    return new LoginOutput
                    {
                        PhoneNumber = user.PhoneNumber,
                        AccessToken = AccountController.OAuthBearerOptions.AccessTokenFormat.Protect(ticket),
                        UserName = user.UserName,
                        Name = user.Name,
                        Avatar = _avatarPath,
                        UserType = user.UserType,
                        UserId = user.Id,
                        EmailAddress = user.EmailAddress,
                        BranchId = user.BranchId,
                        CompanyId = user.CompanyId,
                        ProviderId = user.ProviderId,
                        WorkerId = user.WorkerId,
                        IsActive = user.IsActive,
                        IsClean = IsClean,
                        IsMaintain = IsMaintain,
                        IsOil = IsOil,
                        IsFuel = IsFuel,
                        Success = true,
                        Code = code,
                        FuelPumps = _fuelPump,
                        ForceUpdate = forceUpdate
                    };
                }
                return new LoginOutput { ForceUpdate = forceUpdate };

            }
            catch (Exception ex)
            {
                return new LoginOutput { Message = ex.Message };
            }
        }



        [HttpPost]
        [Language("Lang")]
        public async Task<StringOutput> Logout()
        {
            try
            {
                if (AbpSession.UserId.HasValue && AbpSession.UserId.Value > 0)
                {
                    await _userDeviceRepository.DeleteAsync(x => x.UserId == AbpSession.UserId.Value);
                    return new StringOutput { Success = true, Message = L("MobileApi.Messages.LogoutSuccessfully") };
                }
                return new StringOutput { Success = true, Message = L("Pages.Users.Error.NotExist") };
            }
            catch (Exception ex)
            {
                return new StringOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();


                User user = await _userManager.FindByIdAsync(AbpSession.UserId.Value);
                if (user == null)
                    return Ok(new StringOutput { Message = L("MobileApi.Messages.UserNotExsist") });
                var loginResult = await _logInManager.LoginByUsernameAsync(user.EmailAddress, input.OldPassword, "Default");
                switch (loginResult.Result)
                {
                    case AbpLoginResultType.Success:
                        var Result = await _userManager.ChangePasswordAsync(user, input.NewPassword);
                        if (Result.Succeeded)
                        {
                            return Ok(new StringOutput { Message = L("MobileApi.Messages.PasswordChanged"), Success = true });
                        }
                        else
                            return Ok(new StringOutput { Message = L("MobileApi.Messages.InvalidNewPassword") });
                    case AbpLoginResultType.InvalidPassword:
                        return Ok(new StringOutput { Message = L("MobileApi.Messages.InvalidOldPassword") });
                    default:
                        return Ok(new StringOutput { Message = "Unknown problem with login: " + loginResult.Result });
                }

            }
            catch (Exception ex)
            {
                return Ok(new StringOutput { Message = ex.Message });
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetAllCompanies(GetCompaniesInput input)
        {
            try
            {
                //if (!AbpSession.UserId.HasValue)
                //    return Unauthorized();

                input.MaxCount = true;
                var companies = await _companyAppService.GetAllCompanies(input);
                return Ok(new GetCompaniesOutput { Success = true, Companies = companies });
            }
            catch (Exception ex)
            {
                return Ok(new GetCompaniesOutput { Message = ex.Message, Success = false, Companies = new List<CompanyNameDto>() });
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> GetAllBranchs(GetBranchesInput input)
        {
            try
            {
                //if (!AbpSession.UserId.HasValue)
                //    return Unauthorized();
                input.MaxCount = true;
                var branchs = await _branchAppService.GetAllBranchs(input);
                return Ok(new GetBranchsOutput { Success = true, Branchs = branchs });
            }
            catch (Exception ex)
            {
                return Ok(new GetBranchsOutput { Message = ex.Message, Success = false, Branchs = new List<BranchNameDto>() });
            }
        }

        #endregion

        #region Language
        [HttpPost]
        [Language("Lang")]
        public async Task<IHttpActionResult> ChangeLanguage()
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();
                var Lang = HttpContext.Current.Request.Headers["Lang"];
                //for change user DefaultLanguage
                await _settingManager.ChangeSettingForUserAsync(new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value), "Abp.Localization.DefaultLanguageName", Lang);
                //
                return Ok(new StringOutput { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new StringOutput { Success = false, Message = ex.Message });
            }

        }
        #endregion

        [HttpPost]
        public async Task<SendingResult> SendSms(string phone, string msg)
        {

            SMSHelper smsHelper = new SMSHelper();
            SendMessageInput data = new SendMessageInput
            {
                MessageText = msg,
                PhoneNumbers = phone
            };
            var sendResult = await smsHelper.SendMessage(data);
            return sendResult;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        FileType CheckFileType(string Type)
        {
            switch (Type)
            {
                case "image/png":
                case "image/jpeg":
                case "image/gif":
                case "image/tiff":
                    return FileType.Image;
                //Word
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                //Sql
                case "application/octet-stream":
                //Note Pad
                case "text/plain":
                    return FileType.Doc;
                //Exel
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return FileType.Exel;
                //PDF
                case "application/pdf":
                    return FileType.PDF;
                //VIDEO
                case "video/mp4":
                case "video/x-flv":
                case "application/x-mpegURL":
                case "video/MP2T":
                case "video/3gpp":
                case "video/quicktime":
                case "video/x-msvideo":
                case "video/x-ms-wmv":
                    return FileType.Vedio;
                default:
                    return FileType.None;
            }
        }

    }
}