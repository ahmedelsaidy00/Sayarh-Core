using Abp;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.MultiTenancy;
using Abp.Notifications;
using Abp.Runtime.Security;
using Abp.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sayarah.Api.Models;
using Sayarah.Application.Companies;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Drivers;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Providers;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Users.Dto;
using Sayarah.Authorization;
using Sayarah.Authorization.Users;
using Sayarah.Configuration;
using Sayarah.Core.Helpers;
using Sayarah.MultiTenancy;
using Sayarah.Security;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Sayarah.SayarahConsts;
using SystemClock = Microsoft.Extensions.Internal.SystemClock;

namespace Sayarah.WebApi.Api.Controllers
{
    [ApiController]

    public class AuthController : AbpController
    {
        public AppSession AppSession { get; set; }
        public IHttpContextAccessor HttpContextAccessor { get; }

        // You can inject settings or configs for JWT here
        private readonly string _jwtSecret = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1"; // Store in config securely!
        private readonly string _Issuer = "Sayarh"; // Store in config securely!
        private readonly int _jwtExpirationDays = 3;

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
                  IBranchAppService branchAppService,
                                    IHttpContextAccessor httpContextAccessor

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
            HttpContextAccessor = httpContextAccessor;
        }


        CultureInfo new_lang = new CultureInfo("ar");

        #region User Actions  
        [HttpPost]
        [Language("Lang")]
        public async Task<ActionResult<LoginOutput>> Login(LoginInput input)
        {
            try
            {
                var loginResult = await _logInManager.LoginByUsernameOrEmail(input.UserName, input.Password, GetTenancyNameOrNull());

                switch (loginResult.Result)
                {
                    case AbpLoginResultType.InvalidPassword:
                        return Ok(new LoginOutput { Message = L("MobileApi.Messages.InvalidPassword") });

                    case AbpLoginResultType.UserIsNotActive:
                        return Ok(new LoginOutput { Message = L("MobileApi.Messages.UserNotActive") });

                    case AbpLoginResultType.UserEmailIsNotConfirmed:
                        return Ok(new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) });

                    case AbpLoginResultType.LockedOut:
                        return Ok(new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) });

                    case AbpLoginResultType.Success:
                        return await HandleSuccessfulLogin(loginResult, input);
                }

                return Ok(new LoginOutput { Message = L("MobileApi.Messages.InvalidEmailAddress") });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return Ok(new LoginOutput { Message = ex.Message });
            }
        }

        private async Task<LoginOutput> HandleSuccessfulLogin(AbpLoginResult<Tenant, User> loginResult, LoginInput input)
        {
            if (input.UserType.HasValue && input.UserType.Value != loginResult.User.UserType)
                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };

            if (!IsValidUserType(loginResult.User.UserType))
                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };

            // App version check
            if (loginResult.User.UserType == UserTypes.Worker)
            {
                const string appVersion = "2.1.0";
                if (input.AppVersion == null || input.AppVersion != appVersion)
                {
                    return new LoginOutput { Message = L("MobileApi.Messages.upgradApp") };
                }
            }

            await HandleDeviceRegistration(loginResult.User, input);

            var (avatarPath, code, fuelPump, workerDetails) = await GetUserDetails(loginResult.User);

            var token = await GenerateJwtToken(loginResult.User);

            return new LoginOutput
            {
                PhoneNumber = loginResult.User.PhoneNumber,
                AccessToken = token,
                Name = loginResult.User.Name,
                Avatar = avatarPath,
                UserType = loginResult.User.UserType,
                UserId = loginResult.User.Id,
                EmailAddress = loginResult.User.EmailAddress,
                BranchId = loginResult.User.BranchId,
                CompanyId = loginResult.User.CompanyId,
                ProviderId = loginResult.User.ProviderId,
                WorkerId = loginResult.User.WorkerId,
                UserName = loginResult.User.EmailAddress,
                IsActive = loginResult.User.IsActive,
                IsClean = workerDetails.IsClean,
                IsMaintain = workerDetails.IsMaintain,
                IsOil = workerDetails.IsOil,
                IsFuel = workerDetails.IsFuel,
                Success = true,
                Code = code,
                FuelPumps = fuelPump
            };
        }
        private async Task<(string avatarPath, string code, List<ApiFuelPumpDto> fuelPump, WorkerDetails workerDetails)>
    GetUserDetails(User user)
        {
            string avatarPath = string.Empty;
            string code = string.Empty;
            List<ApiFuelPumpDto> fuelPump = new List<ApiFuelPumpDto>();
            var workerDetails = new WorkerDetails(); // Helper class

            if (user.UserType == UserTypes.Driver)
            {
                // Driver-specific logic
                avatarPath = await GetDriverAvatar(user.Avatar);
                var driver = await _driverAppService.GetByUserId(new EntityDto<long> { Id = user.Id });
                code = driver?.Code ?? string.Empty;

                if (driver?.Branch == null || driver?.Branch.Company == null ||
                    !driver.Branch.User.IsActive || !driver.Branch.Company.User.IsActive)
                {
                    throw new UserFriendlyException(L("MobileApi.Messages.BranchOrCompanyNotActive"));
                }
            }
            else if (user.UserType == UserTypes.Worker)
            {
                // Worker-specific logic
                avatarPath = await GetWorkerAvatar(user.Avatar);
                var worker = await _workerAppService.GetByUserId(new EntityDto<long> { Id = user.Id });

                code = worker?.Code ?? string.Empty;
                workerDetails = new WorkerDetails
                {
                    IsFuel = worker?.IsFuel ?? false,
                    IsOil = worker?.IsOil ?? false,
                    IsClean = worker?.IsClean ?? false,
                    IsMaintain = worker?.IsMaintain ?? false
                };

                if (user.ProviderId.HasValue)
                {
                    var fuelPumps = await _fuelPumpAppService.GetAllAsync(
                        new GetFuelPumpsInput
                        {
                            ProviderId = user.ProviderId,
                            MaxCount = true
                        });
                    fuelPump = ObjectMapper.Map<List<ApiFuelPumpDto>>(fuelPumps.Items);
                }

                if (worker?.Provider == null || worker?.Provider.MainProvider == null ||
                    !worker.Provider.User.IsActive || !worker.Provider.MainProvider.User.IsActive)
                {
                    throw new UserFriendlyException(L("MobileApi.Messages.BranchOrCompanyNotActive"));
                }
            }

            return (avatarPath, code, fuelPump, workerDetails);
        }

        // Helper class to organize worker details
        private class WorkerDetails
        {
            public bool IsFuel { get; set; }
            public bool IsOil { get; set; }
            public bool IsClean { get; set; }
            public bool IsMaintain { get; set; }
        }

        // Avatar path helpers
        private async Task<string> GetDriverAvatar(string avatar)
        {
            if (!string.IsNullOrEmpty(avatar) &&
                 Utilities.CheckExistImage(6, $"600x600_{avatar}"))
            {
                return FilesPath.Drivers.ServerImagePath + $"600x600_{avatar}";
            }
            return FilesPath.Drivers.DefaultImagePath;
        }

        private async Task<string> GetWorkerAvatar(string avatar)
        {
            if (!string.IsNullOrEmpty(avatar) &&
                 Utilities.CheckExistImage(8, $"600x600_{avatar}"))
            {
                return FilesPath.Workers.ServerImagePath + $"600x600_{avatar}";
            }
            return FilesPath.Workers.DefaultImagePath;
        }


        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        // ABP Required Claims
        new Claim(AbpClaimTypes.UserId, user.Id.ToString()),
        new Claim(AbpClaimTypes.UserName, user.UserName),
        new Claim(ClaimTypes.Email, user.EmailAddress),
        new Claim(AbpClaimTypes.TenantId, user.TenantId?.ToString() ?? ""),
        
        // Standard JWT Claims
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

            // Add roles (both ABP and standard)
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(AbpClaimTypes.Role, role));
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_jwtExpirationDays));
            var token = new JwtSecurityToken(
                issuer: "Sayarah",
                audience: "Sayarah",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper methods
        private bool IsValidUserType(UserTypes userType)
        {
            return userType switch
            {
                UserTypes.Driver => true,
                UserTypes.Worker => true,
                UserTypes.AdminEmployee => true,
                UserTypes.Company => true,
                _ => false
            };
        }

        private async Task HandleDeviceRegistration(User user, LoginInput input)
        {
            var userDevices = await _userDeviceRepository.GetAllListAsync(x => x.UserId == user.Id);

            foreach (var device in userDevices.Where(d => d.RegistrationToken != input.RegistrationToken))
            {
                await SendLogoutNotification(device.RegistrationToken);
                await _userDeviceRepository.DeleteAsync(device);
            }

            var existingDevice = await _userDeviceRepository.FirstOrDefaultAsync(x =>
                x.RegistrationToken == input.RegistrationToken &&
                x.DeviceType == input.DeviceType);

            if (existingDevice != null)
            {
                existingDevice.UserId = user.Id;
                await _userDeviceRepository.UpdateAsync(existingDevice);
            }
            else
            {
                await _userDeviceRepository.InsertAsync(new UserDevice
                {
                    UserId = user.Id,
                    RegistrationToken = input.RegistrationToken,
                    DeviceType = input.DeviceType
                });
            }
        }

        private async Task SendLogoutNotification(string registrationToken)
        {
            var notificationInput = new FcmNotificationInput
            {
                RegistrationToken = registrationToken,
                Title = L("Common.SystemTitle"),
                Type = FcmNotificationType.Logout,
                Body = L("MobileApi.Messages.IsLogedinFromAnthorDevice")
            };
            FCMPushNotification _fcmPushNotification = new FCMPushNotification();

            await _fcmPushNotification.SendNotification(notificationInput);
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<LoginOutput> SimAppLogin(LoginInput input)
        {
            try
            {
                var loginResult = await _logInManager.LoginByUsernameOrEmail(input.UserName, input.Password, GetTenancyNameOrNull());

                switch (loginResult.Result)
                {
                    case AbpLoginResultType.InvalidPassword:
                        return new LoginOutput { Message = L("MobileApi.Messages.InvalidPassword") };

                    case AbpLoginResultType.UserIsNotActive:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive") };

                    case AbpLoginResultType.UserEmailIsNotConfirmed:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) };

                    case AbpLoginResultType.LockedOut:
                        return new LoginOutput { Message = L("MobileApi.Messages.UserNotActive", new_lang) };

                    case AbpLoginResultType.Success:
                        if (loginResult.User.UserType != UserTypes.Admin &&
                            loginResult.User.UserType != UserTypes.Company &&
                            loginResult.User.UserType != UserTypes.Employee)
                        {
                            return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                        }

                        if (loginResult.User.UserType == UserTypes.Employee)
                        {
                            if (!loginResult.User.CompanyId.HasValue)
                                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };

                            var company = await _companyAppService.GetAsync(new EntityDto<long> { Id = loginResult.User.CompanyId.Value });
                            if (company == null || company.IsDeleted || !company.User.IsActive)
                                return new LoginOutput { Message = L("MobileApi.Messages.UserTypeNotValid") };
                        }

                        // Generate JWT token directly
                        var claims = new List<Claim>
                {
                    new Claim(AbpClaimTypes.UserId, loginResult.User.Id.ToString()),
                    new Claim(AbpClaimTypes.UserName, loginResult.User.UserName),
                    new Claim("EmailAddress", loginResult.User.EmailAddress),
                    new Claim(AbpClaimTypes.TenantId, loginResult.User.TenantId?.ToString() ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                        // Add roles directly
                        var roles = await _userManager.GetRolesAsync(loginResult.User);
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_jwtExpirationDays));

                        var token = new JwtSecurityToken(
                            issuer: _Issuer,
                            audience: _Issuer,
                            claims: claims,
                            expires: expires,
                            signingCredentials: creds
                        );

                        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                        // Device registration (original code preserved)
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

                        // Avatar handling (original code preserved)
                        string _avatarPath = string.Empty;
                        if (loginResult.User.UserType == UserTypes.Admin || loginResult.User.UserType == UserTypes.Employee)
                        {
                            _avatarPath = !string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(1, "400x400_" + loginResult.User.Avatar)
                                ? FilesPath.Users.ServerImagePath + "400x400_" + loginResult.User.Avatar
                                : FilesPath.Users.DefaultImagePath;
                        }
                        else if (loginResult.User.UserType == UserTypes.Company)
                        {
                            _avatarPath = !string.IsNullOrEmpty(loginResult.User.Avatar) && Utilities.CheckExistImage(4, "600x600_" + loginResult.User.Avatar)
                                ? FilesPath.Companies.ServerImagePath + "600x600_" + loginResult.User.Avatar
                                : FilesPath.Companies.DefaultImagePath;
                        }

                        return new LoginOutput
                        {
                            PhoneNumber = loginResult.User.PhoneNumber,
                            AccessToken = tokenString, // JWT token here
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
                bool forceUpdate = await SettingManager.GetSettingValueAsync(AppSettingNames.ForceUpdate) == "false" ? false : true;

                var userId = await _userAppService.UserGetByDevice(new GetUserByDeviceInput
                {
                    DeviceType = input.DeviceType,
                    RegistrationToken = input.RegistrationToken
                });

                if (userId <= 0) return new LoginOutput { ForceUpdate = forceUpdate };

                var user = await _userManager.GetUserByIdAsync(userId);
                if (user == null) return new LoginOutput { };

                // Generate JWT token directly

                var tokenString = await GenerateJwtToken(user);

                // Original avatar and user data logic
                string _avatarPath = string.Empty;
                bool IsFuel = false, IsOil = false, IsClean = false, IsMaintain = false;
                string code = string.Empty;
                List<ApiFuelPumpDto> _fuelPump = new List<ApiFuelPumpDto>();

                if (user.UserType == UserTypes.Driver)
                {
                    _avatarPath = !string.IsNullOrEmpty(user.Avatar) && Utilities.CheckExistImage(6, "600x600_" + user.Avatar)
                        ? FilesPath.Drivers.ServerImagePath + "600x600_" + user.Avatar
                        : FilesPath.Drivers.DefaultImagePath;

                    var driver = await _driverAppService.GetByUserId(new EntityDto<long> { Id = user.Id });
                    if (driver != null)
                    {
                        code = driver.Code;
                        if (driver.Branch == null || driver.Branch.Company == null ||
                            !driver.Branch.User.IsActive || !driver.Branch.Company.User.IsActive)
                        {
                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") };
                        }
                    }
                }
                else if (user.UserType == UserTypes.Worker)
                {
                    _avatarPath = !string.IsNullOrEmpty(user.Avatar) && Utilities.CheckExistImage(8, "600x600_" + user.Avatar)
                        ? FilesPath.Workers.ServerImagePath + "600x600_" + user.Avatar
                        : FilesPath.Workers.DefaultImagePath;

                    var worker = await _workerAppService.GetByUserId(new EntityDto<long> { Id = user.Id });
                    if (worker != null)
                    {
                        IsFuel = worker.IsFuel;
                        IsOil = worker.IsOil;
                        IsClean = worker.IsClean;
                        IsMaintain = worker.IsMaintain;
                        code = worker.Code;

                        if (worker.Provider == null || worker.Provider.MainProvider == null ||
                            !worker.Provider.User.IsActive || !worker.Provider.MainProvider.User.IsActive)
                        {
                            return new LoginOutput { Message = L("MobileApi.Messages.BranchOrCompanyNotActive") };
                        }
                    }

                    if (user.ProviderId.HasValue)
                    {
                        var fuelPumps = await _fuelPumpAppService.GetAllAsync(
                            new GetFuelPumpsInput { ProviderId = user.ProviderId, MaxCount = true });
                        _fuelPump = ObjectMapper.Map<List<ApiFuelPumpDto>>(fuelPumps.Items);
                    }
                }

                int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(
                    new UserIdentifier(AbpSession.TenantId, user.Id),
                    UserNotificationState.Unread);

                return new LoginOutput
                {
                    PhoneNumber = user.PhoneNumber,
                    AccessToken = tokenString, // JWT token here
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
        public async Task<IActionResult> ChangePassword(ChangePasswordInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();


                User user = await _userManager.FindByIdAsync(AbpSession.UserId.ToString());
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
        public async Task<IActionResult> GetAllCompanies(GetCompaniesInput input)
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
        public async Task<IActionResult> GetAllBranchs(GetBranchesInput input)
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
        public async Task<IActionResult> ChangeLanguage()
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    return Unauthorized();

                var Lang = HttpContextAccessor?.HttpContext?.Request?.Headers["Lang"].ToString();

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