using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.UI;
using Abp.Web.Models;
using Sayarah.Authorization;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.MultiTenancy;
using Sayarah.Sessions;
using Sayarah.Web.Controllers.Results;
using Sayarah.Web.Models;
using Sayarah.Web.Models.Account;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using static Sayarah.SayarahConsts;
using Sayarah.Helpers;
using Microsoft.Owin.Security.DataProtection;
using System.Globalization;
using Abp.Domain.Repositories;
using Abp.Application.Services.Dto;
using Sayarah.Helpers.Enums;
using Sayarah.Companies;
using Sayarah.Providers;
using System.ComponentModel.DataAnnotations;
using Sayarah.Application.Sessions;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace Sayarah.Web.Controllers
{
    public class AccountController : SayarahControllerBase
    {
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly LogInManager _logInManager;
        private readonly ISessionAppService _sessionAppService;
        private readonly ILanguageManager _languageManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly ICommonAppService _commonAppService;
        private readonly IRepository<User, long> _userRepository;

        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AccountController(
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IUnitOfWorkManager unitOfWorkManager,
            IMultiTenancyConfig multiTenancyConfig,
            LogInManager logInManager,
            ISessionAppService sessionAppService,
            ILanguageManager languageManager,
            ITenantCache tenantCache,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService,
            IRepository<User, long> userRepository,
            IRepository<Company, long> companyRepository,
            IRepository<MainProvider, long> mainProviderRepository,
            IRepository<Branch, long> branchRepository,
            IRepository<Provider, long> providerRepository
            )
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWorkManager = unitOfWorkManager;
            _multiTenancyConfig = multiTenancyConfig;
            _logInManager = logInManager;
            _sessionAppService = sessionAppService;
            _languageManager = languageManager;
            _tenantCache = tenantCache;
            _abpNotificationHelper = abpNotificationHelper;
            _commonAppService = commonAppService;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mainProviderRepository = mainProviderRepository;
            _branchRepository = branchRepository;
            _providerRepository = providerRepository;
        }

        #region Login / Logout

        [DisableAuditing]
        public ActionResult Login(string returnUrl = "")
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Request.ApplicationPath;
            }

            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;

            return View(
                new LoginFormViewModel
                {
                    ReturnUrl = returnUrl,
                    IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
                    IsSelfRegistrationAllowed = IsSelfRegistrationEnabled(),
                    MultiTenancySide = AbpSession.MultiTenancySide
                });
        }

        [HttpPost]
        [DisableAuditing]
        public async Task<JsonResult> Login(LoginViewModel loginModel, string returnUrl = "", string returnUrlHash = "")
        {
            CheckModelState();

            var loginResult = await GetLoginResultAsync(
                loginModel.UsernameOrEmailAddress,
                loginModel.Password,
                GetTenancyNameOrNull()
            );

            var ticket = new AuthenticationTicket(loginResult.Identity, new AuthenticationProperties());
            //
            IEnumerable<Claim> rolesList = ticket.Identity.Claims;
            var roleClaim = rolesList.FirstOrDefault(c => c.Type == ticket.Identity.RoleClaimType);
            if (roleClaim.Value != RolesNames.Client && roleClaim.Value != RolesNames.Worker && roleClaim.Value != RolesNames.Driver)
            {
                await SignInAsync(loginResult.User, loginResult.Identity, loginModel.RememberMe);
            }
            else
                throw CreateExceptionForFailedLoginAttempt(AbpLoginResultType.InvalidUserNameOrEmailAddress, loginModel.UsernameOrEmailAddress, "Default");

            // check if user 
            await CheckUsers(loginResult.User.UserType, loginResult.User.CompanyId, loginResult.User.BranchId, loginResult.User.MainProviderId, loginResult.User.ProviderId);

            returnUrl = "/admin";
            // 

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Request.ApplicationPath;
            }

            if (!string.IsNullOrWhiteSpace(returnUrlHash))
            {
                returnUrl = returnUrl + returnUrlHash;
            }

            return Json(new AjaxResponse { TargetUrl = returnUrl });
        }


        [HttpPost]
        [DisableAuditing]
        public async Task CheckUsers(UserTypes _userType, long? companyId, long? branchId, long? mainProviderId, long? providerId)
        {


            // check if user 
            if (_userType == UserTypes.Branch)
            {
                // check if company is deleted or not active 
                var _company = await _companyRepository.GetAll().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == companyId);
                if (_company == null || _company.User == null)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                if (_company.IsDeleted == true || _company.User.IsDeleted == true)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                if (_company.User.IsActive == false)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
            }
            else if (_userType == UserTypes.Employee)
            {
                // هشوف الاول دا موظف فرع و لا شركة
                if (companyId.HasValue == true)
                {
                    // check if company is deleted or not active 
                    var _company = await _companyRepository.GetAll().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == companyId);
                    if (_company == null || _company.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_company.IsDeleted == true || _company.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_company.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
                }
                else if (branchId.HasValue == true)
                {
                    // check if company is deleted or not active 
                    var _branch = await _branchRepository.GetAll()
                        .Include(a => a.User)
                        .Include(a => a.Company.User)
                        .FirstOrDefaultAsync(a => a.Id == branchId);

                    if (_branch == null || _branch.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_branch.Company == null || _branch.Company.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));


                    if (_branch.IsDeleted == true || _branch.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_branch.Company.IsDeleted == true || _branch.Company.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_branch.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_branch.Company.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
                }

                else if (mainProviderId.HasValue == true)
                {
                    // check if company is deleted or not active 
                    var _mainProvider = await _mainProviderRepository.GetAll().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == mainProviderId);
                    if (_mainProvider == null || _mainProvider.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_mainProvider.IsDeleted == true || _mainProvider.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_mainProvider.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
                }
                else if (providerId.HasValue == true)
                {
                    // check if company is deleted or not active 
                    var _provider = await _providerRepository.GetAll()
                        .Include(a => a.User)
                        .Include(a => a.MainProvider.User)
                        .FirstOrDefaultAsync(a => a.Id == providerId);

                    if (_provider == null || _provider.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_provider.MainProvider == null || _provider.MainProvider.User == null)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));


                    if (_provider.IsDeleted == true || _provider.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_provider.MainProvider.IsDeleted == true || _provider.MainProvider.User.IsDeleted == true)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                    if (_provider.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateBranch"));

                    if (_provider.MainProvider.User.IsActive == false)
                        throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
                }
            }

            else if (_userType == UserTypes.Provider)
            {
                // check if company is deleted or not active 
                var _company = await _mainProviderRepository.GetAll().Include(a => a.User).FirstOrDefaultAsync(a => a.Id == mainProviderId);
                if (_company == null || _company.User == null)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                if (_company.IsDeleted == true || _company.User.IsDeleted == true)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));

                if (_company.User.IsActive == false)
                    throw new UserFriendlyException(L("Messages.LoginFailedDeactivateCompany"));
            }

        }



        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private async Task SignInAsync(User user, ClaimsIdentity identity = null, bool rememberMe = false)
        {
            if (identity == null)
            {
                identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            // Gp - fix code for NOT using session cookies
            // Don’t rely solely on browser behaviour for proper clean-up of session cookies during a given browsing session. 
            // It’s safer to use non-session cookies (IsPersistent == true) with an expiration date for having a 
            // consistent behaviour across all browsers and versions.
            // See http://blog.petersondave.com/cookies/Session-Cookies-in-Chrome-Firefox-and-Sitecore/

            // Gp Commented out: AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, identity);
            if (rememberMe)
            {
                //var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id.ToString());
                AuthenticationManager.SignIn(
                    new AuthenticationProperties { IsPersistent = true },
                    identity /*, rememberBrowserIdentity*/);
            }
            else
            {
                AuthenticationManager.SignIn(
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc =
                            DateTimeOffset.UtcNow.AddDays(365)
                    },
                    identity);
            }
        }

        private Exception CreateExceptionForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    return new ApplicationException("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return new UserFriendlyException(L("Messages.LoginFailed"), L("Messages.InvalidLogin"));
                case AbpLoginResultType.InvalidTenancyName:
                    return new UserFriendlyException(L("Messages.LoginFailed"), L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                case AbpLoginResultType.TenantIsNotActive:
                    return new UserFriendlyException(L("Messages.LoginFailed"), L("TenantIsNotActive", tenancyName));
                case AbpLoginResultType.UserIsNotActive:
                    return new UserFriendlyException(L("Messages.LoginFailed"), L("UserIsNotActiveAndCanNotLogin", usernameOrEmailAddress));
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return new UserFriendlyException(L("Messages.LoginFailed"), "UserEmailIsNotConfirmedAndCanNotLogin");
                case AbpLoginResultType.LockedOut:
                    return new UserFriendlyException(L("Messages.LoginFailed"), L("UserLockedOutMessage"));
                default: //Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return new UserFriendlyException(L("LoginFailed"));
            }
        }

        private JsonResult CreateExceptionForFailedLogin(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    return Json(new AjaxResponse { Success = true });
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(0, L("Messages.InvalidLogin")) });
                case AbpLoginResultType.InvalidPassword:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(0, L("Messages.InvalidLogin")) });
                case AbpLoginResultType.InvalidTenancyName:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(1, L("Messages.InvalidTenant", tenancyName)) });
                case AbpLoginResultType.TenantIsNotActive:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(1, L("Messages.TenantIsNotActive", tenancyName)) });
                case AbpLoginResultType.UserIsNotActive:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(0, L("Messages.UserIsNotActive", usernameOrEmailAddress)) });
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(0, L("LoginFailed"), L("UserEmailIsNotConfirmedAndCanNotLogin")) });
                case AbpLoginResultType.LockedOut:
                    return Json(new AjaxResponse { Success = false, Error = new ErrorInfo(0, L("LoginFailed"), L("UserLockedOutMessage")) });
                default: //Can not fall to default for now. But other result types can be added in the future and we may forget to handle it
                    throw new UserFriendlyException("Unknown problem with login: " + result);
            }
        }

        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        #endregion

        #region Register

 



        public ActionResult Register()
        {
            return RegisterView(new RegisterViewModel());
        }

       

        private ActionResult RegisterView(RegisterViewModel model)
        {
            ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
            ViewBag.StoreBranchesList = new List<StoreBranchItem>();

            return View("Register", model);
        }

        private bool IsSelfRegistrationEnabled()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return false; //No registration enabled for host users!
            }

            return true;
        }

        [HttpPost]
        public virtual async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                CheckModelState();

                //Create user
                var user = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    EmailAddress = model.EmailAddress,
                    IsActive = true
                };

                //Get external login info if possible
                ExternalLoginInfo externalLoginInfo = null;
                if (model.IsExternalLogin)
                {
                    externalLoginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                    if (externalLoginInfo == null)
                    {
                        throw new ApplicationException("Can not external login!");
                    }

                    user.Logins = new List<UserLogin>
                    {
                        new UserLogin
                        {
                            LoginProvider = externalLoginInfo.Login.LoginProvider,
                            ProviderKey = externalLoginInfo.Login.ProviderKey
                        }
                    };

                    if (model.UserName.IsNullOrEmpty())
                    {
                        model.UserName = model.EmailAddress;
                    }

                    model.Password = Authorization.Users.User.CreateRandomPassword();

                    if (string.Equals(externalLoginInfo.Email, model.EmailAddress, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user.IsEmailConfirmed = true;
                    }
                }
                else
                {
                    //Username and Password are required if not external login
                    if (model.UserName.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
                    {
                        throw new UserFriendlyException(L("FormIsNotValidMessage"));
                    }
                }

                user.UserName = model.UserName;
                user.Password = new PasswordHasher().HashPassword(model.Password);

                //Switch to the tenant
                _unitOfWorkManager.Current.EnableFilter(AbpDataFilters.MayHaveTenant); //TODO: Needed?
                _unitOfWorkManager.Current.SetTenantId(AbpSession.GetTenantId());

                //Add default roles
                user.Roles = new List<UserRole>();
                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    user.Roles.Add(new UserRole { RoleId = defaultRole.Id });
                }

                //Save user
                CheckErrors(await _userManager.CreateAsync(user));
                await _unitOfWorkManager.Current.SaveChangesAsync();

                //Directly login if possible
                if (user.IsActive)
                {
                    AbpLoginResult<Tenant, User> loginResult;
                    if (externalLoginInfo != null)
                    {
                        loginResult = await _logInManager.LoginAsync(externalLoginInfo.Login, GetTenancyNameOrNull());
                    }
                    else
                    {
                        loginResult = await GetLoginResultAsync(user.UserName, model.Password, GetTenancyNameOrNull());
                    }

                    if (loginResult.Result == AbpLoginResultType.Success)
                    {
                        await SignInAsync(loginResult.User, loginResult.Identity);
                        return Redirect(Url.Action("Index", "Home"));
                    }

                    Logger.Warn("New registered user could not be login. This should not be normally. login result: " + loginResult.Result);
                }

                //If can not login, show a register result page
                return View("RegisterResult", new RegisterResultViewModel
                {
                    TenancyName = GetTenancyNameOrNull(),
                    NameAndSurname = user.Name + " " + user.Surname,
                    UserName = user.UserName,
                    EmailAddress = user.EmailAddress,
                    IsActive = user.IsActive
                });
            }
            catch (UserFriendlyException ex)
            {
                ViewBag.IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled;
                ViewBag.ErrorMessage = ex.Message;

                return View("Register", model);
            }
        }

        public async Task<JsonResult> ConfirmRegisteration(long token, string code)
        {
            try
            {
                User user = await _userManager.GetUserByIdAsync(token);

                if (user != null)
                {
                    if (user.IsEmailConfirmed)
                    {
                        return Json(new
                        {
                            Success = true,
                            Message = L("Pages.ConfirmRegisteration.AlreadyActivated")
                        });
                    }
                    else
                    {
                        if (user.EmailConfirmationCode == code)
                        {
                            user.SetCodeConfirmed();
                            user.IsActive = true;
                            user.IsEmailConfirmed = true;
                            await _userManager.UpdateAsync(user);
                            await _unitOfWorkManager.Current.SaveChangesAsync();
                            AuthenticationManager.SignOut();

                            return Json(new
                            {
                                Success = true,
                                Message = L("Pages.ConfirmEmail.AccountActivated")
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                Success = false,
                                Message = L("Pages.ConfirmRegisteration.NotFound")

                            });
                        }
                    }
                }
                return Json(new
                {
                    Success = false,
                    Message = L("Pages.ConfirmRegisteration.NotFound")

                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = new ErrorInfo(ex.Message)
                });
            }
        }

        #endregion

        #region ForgotPassword AngularJs

        // POST: /Account/ForgotPassword
        //[HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel input)
        {
            try
            {
                CultureInfo new_lang = new CultureInfo((!string.IsNullOrEmpty(input.Lang) && input.Lang.Equals("ar")) ? "ar" : "en");
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(input.EmailAddress);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return Json(new { Success = false, Message = new ErrorInfo(L("Common.Error.InvalidEmail", new_lang)) });
                    }

                    ////As U Need To User////
                    //IList<string> _userRoles = await _userManager.GetRolesAsync(user.Id);
                    //if (_userRoles != null && _userRoles.Count > 0)
                    //{
                    //    switch (_userRoles[0])
                    //    {
                    //        case StaticRoleNames.Host.Admin:
                    //            return Json(new { Success = false, Error = new ErrorInfo(L("Pages.Login.InvalidRole", new_lang)) });

                    //        default:
                    //            break;
                    //    }
                    //}

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    var dataProtectionProvider = new DpapiDataProtectionProvider(SayarahConsts.DataProtectionProviderName);
                    _userManager.UserTokenProvider = new DataProtectorTokenProvider<User, long>(dataProtectionProvider.Create("ASP.NET Identity")) as IUserTokenProvider<User, long>;
                    string code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                    code = HttpUtility.UrlEncode(code);
                    var callbackUrl = input.LocationHost + "/#/resetPassword?id=" + code;
                    try
                    {
                        bool result = await _commonAppService.SendEmail(new SendEmailRequest
                        {
                            Subject = new_lang.DisplayName == "en" ? "Reset Password" : "إعادة تعيين كلمة المرور",
                            datalst = new[] { new_lang.DisplayName == "en" ? "You recently requested a password reset for your account.Click the button below to reset it" : "طلبت مؤخرًا إعادة تعيين كلمة المرور لحسابك.انقر فوق الزر أدناه لإعادة تعيينه" },
                            Emails = new[] { input.EmailAddress },
                            Url = callbackUrl,
                            UrlTitle = "Pages.ResetPassword.ResetPassword"
                        });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { Success = false, Message = new ErrorInfo(L("Pages.Users.Messages.FailedSendEmail", new_lang)) });
                    }

                    return Json(new { Success = true, Message = L("Pages.ResetPassword.checkEmail", new_lang) });
                }

                // If we got this far, something failed, redisplay form
                return Json(new { Success = false, Message = new ErrorInfo(L("FormIsNotValidMessage", new_lang)) });
            }
            catch (UserFriendlyException ex)
            {
                return Json(new { Success = false, Message = new ErrorInfo(ex.Message) });
            }
        }
        //
        // POST: /Account/ResetPassword
        //  [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel input)
        {
            CultureInfo new_lang = new CultureInfo((!string.IsNullOrEmpty(input.Lang) && input.Lang.Equals("ar")) ? "ar" : "en");
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = new ErrorInfo(L("FormIsNotValidMessage", new_lang)) });
            }

            var user = await _userManager.FindByEmailAsync(input.EmailAddress);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Json(new { Success = false, Message = new ErrorInfo(L("Common.Error.InvalidEmail", new_lang)) });
            }

            var dataProtectionProvider = new DpapiDataProtectionProvider(SayarahConsts.DataProtectionProviderName);
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<User, long>(dataProtectionProvider.Create("ASP.NET Identity")) as IUserTokenProvider<User, long>;
            var result = await _userManager.ResetPasswordAsync(user.Id, HttpUtility.UrlDecode(input.Code), input.Password);
            if (result.Succeeded)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Message = new ErrorInfo(((string[])(result.Errors))[0]) });
                ///OR///
                //return Json(new { Success = false, Message = new ErrorInfo(L("ValidationError")) });
            }
        }

        #endregion

        #region External Login

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(
                provider,
                Url.Action(
                    "ExternalLoginCallback",
                    "Account",
                    new
                    {
                        ReturnUrl = returnUrl
                    })
            );
        }

        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl, string tenancyName = "")
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            //Try to find tenancy name
            if (tenancyName.IsNullOrEmpty())
            {
                var tenants = await FindPossibleTenantsOfUserAsync(loginInfo.Login);
                switch (tenants.Count)
                {
                    case 0:
                        return await RegisterView(loginInfo);
                    case 1:
                        tenancyName = tenants[0].TenancyName;
                        break;
                    default:
                        return View("TenantSelection", new TenantSelectionViewModel
                        {
                            Action = Url.Action("ExternalLoginCallback", "Account", new { returnUrl }),
                            Tenants = ObjectMapper.Map<List<TenantSelectionViewModel.TenantInfo>>(tenants)
                        });
                }
            }

            var loginResult = await _logInManager.LoginAsync(loginInfo.Login, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    await SignInAsync(loginResult.User, loginResult.Identity);

                    if (string.IsNullOrWhiteSpace(returnUrl))
                    {
                        returnUrl = Url.Action("Index", "Home");
                    }

                    return Redirect(returnUrl);
                case AbpLoginResultType.UnknownExternalLogin:
                    return await RegisterView(loginInfo, tenancyName);
                default:
                    throw CreateExceptionForFailedLoginAttempt(loginResult.Result, loginInfo.Email ?? loginInfo.DefaultUserName, tenancyName);
            }
        }

        private async Task<ActionResult> RegisterView(ExternalLoginInfo loginInfo, string tenancyName = null)
        {
            var name = loginInfo.DefaultUserName;
            var surname = loginInfo.DefaultUserName;

            var extractedNameAndSurname = TryExtractNameAndSurnameFromClaims(loginInfo.ExternalIdentity.Claims.ToList(), ref name, ref surname);

            var viewModel = new RegisterViewModel
            {
                EmailAddress = loginInfo.Email,
                Name = name,
                Surname = surname,
                IsExternalLogin = true
            };

            if (!tenancyName.IsNullOrEmpty() && extractedNameAndSurname)
            {
                return await Register(viewModel);
            }

            return RegisterView(viewModel);
        }

        protected virtual async Task<List<Tenant>> FindPossibleTenantsOfUserAsync(UserLoginInfo login)
        {
            List<User> allUsers;
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                allUsers = await _userManager.FindAllAsync(login);
            }

            return allUsers
                .Where(u => u.TenantId != null)
                .Select(u => AsyncHelper.RunSync(() => _tenantManager.FindByIdAsync(u.TenantId.Value)))
                .ToList();
        }

        private static bool TryExtractNameAndSurnameFromClaims(List<Claim> claims, ref string name, ref string surname)
        {
            string foundName = null;
            string foundSurname = null;

            var givennameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            if (givennameClaim != null && !givennameClaim.Value.IsNullOrEmpty())
            {
                foundName = givennameClaim.Value;
            }

            var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            if (surnameClaim != null && !surnameClaim.Value.IsNullOrEmpty())
            {
                foundSurname = surnameClaim.Value;
            }

            if (foundName == null || foundSurname == null)
            {
                var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nameClaim != null)
                {
                    var nameSurName = nameClaim.Value;
                    if (!nameSurName.IsNullOrEmpty())
                    {
                        var lastSpaceIndex = nameSurName.LastIndexOf(' ');
                        if (lastSpaceIndex < 1 || lastSpaceIndex > (nameSurName.Length - 2))
                        {
                            foundName = foundSurname = nameSurName;
                        }
                        else
                        {
                            foundName = nameSurName.Substring(0, lastSpaceIndex);
                            foundSurname = nameSurName.Substring(lastSpaceIndex);
                        }
                    }
                }
            }

            if (!foundName.IsNullOrEmpty())
            {
                name = foundName;
            }

            if (!foundSurname.IsNullOrEmpty())
            {
                surname = foundSurname;
            }

            return foundName != null && foundSurname != null;
        }

        #endregion

        #region Common private methods

        private async Task<Tenant> GetActiveTenantAsync(string tenancyName)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIsNotActive", tenancyName));
            }

            return tenant;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        #endregion

        #region Common Partial Views


        [ChildActionOnly]
        public PartialViewResult TenantChange()
        {
            var loginInformations = AsyncHelper.RunSync(() => _sessionAppService.GetCurrentLoginInformations());

            return PartialView("_TenantChange", new TenantChangeViewModel
            {
                Tenant = loginInformations.Tenant
            });
        }

        public async Task<PartialViewResult> TenantChangeModal()
        {
            var loginInfo = await _sessionAppService.GetCurrentLoginInformations();
            return PartialView("_TenantChangeModal", new TenantChangeModalViewModel
            {
                TenancyName = loginInfo.Tenant?.TenancyName
            });
        }


        [ChildActionOnly]
        public PartialViewResult _AccountLanguages()
        {
            var model = new LanguageSelectionViewModel
            {
                CurrentLanguage = _languageManager.CurrentLanguage,

                Languages = _languageManager.GetLanguages().Where(l => !l.IsDisabled).ToList()
                    .Where(l => !l.IsDisabled && (l.Name == "ar" || l.Name == "en"))
                    .ToList(),
                CurrentUrl = Request.Path
            };

            return PartialView("_AccountLanguages", model);
        }

        private async Task<Tenant> GetActiveTenantAsync()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return await GetActiveTenantAsync(AbpSession.TenantId.Value);
        }

        private async Task<Tenant> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }
        #endregion


        #region Front


        [HttpPost]
        [DisableAuditing]
        public async Task<JsonResult> ConfirmConfirmationCode(ConfirmConfirmationCodeModel input)
        {
            CheckModelState();

            var user = await _userRepository.FirstOrDefaultAsync(a => a.PhoneNumber == input.PhoneNumber && a.IsDeleted == false);

            if (user == null)
                return Json(new { Success = false, Message = L("MobileApi.Messages.UserNotfound") });


            if (user.EmailConfirmationCode != input.ConfirmationCode)
                return Json(new { Success = false, Message = L("MobileApi.Messages.ConfirmationCodeError") });

            if (user.IsActive == false)
                return Json(new { Success = false, Message = L("MobileApi.Messages.UserNotActive") });


            user.SetCodeConfirmed();
            user.IsActive = true;
            user.IsPhoneNumberConfirmed = true;
            user.IsDeleted = false;
            await _userManager.UpdateAsync(user);

            // user devices 

            await _unitOfWorkManager.Current.SaveChangesAsync();


            var loginResult = await _logInManager.LoginByPhoneFront(input.PhoneNumber, GetTenancyNameOrNull());



            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    break;
                case AbpLoginResultType.UserIsNotActive:
                    return Json(new { Success = false, Message = L("Messages.UserIsNotActive", input.PhoneNumber) });

                default: //Can not fall to default for now. But other result types can be added in the future and we may forget to handle it
                    return Json(new { Success = false, Message = "Unknown problem with login: " + loginResult.Result });
            }

            await SignInAsync(loginResult.User, loginResult.Identity, true);

            if (string.IsNullOrWhiteSpace(input.ReturnUrl))
            {
                input.ReturnUrl = Request.ApplicationPath;
            }

            return Json(new { Success = true, TargetUrl = input.ReturnUrl });
        }


        // login by phone and password 



        [HttpPost]
        [DisableAuditing]
        public async Task<JsonResult> FrontLogin(FrontLoginModel input)
        {
            CheckModelState();

            //get user first 

            var loginResult = await _logInManager.LoginByPhoneAsync(input.PhoneNumber, input.Password, GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    break;

                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                    return Json(new { Success = false, Message = L("MobileApi.Messages.InvalidPhoneNumber") });

                case AbpLoginResultType.InvalidPassword:
                    return Json(new { Success = false, Message = L("MobileApi.Messages.InvalidPassword") });

                case AbpLoginResultType.UserPhoneNumberIsNotConfirmed:

                    // resend confirmation code and confirm this phone first 
                    var _sendSmsResult = await ResendConfirmationCode(new FrontLoginModel { PhoneNumber = input.PhoneNumber });
                    return Json(new
                    {
                        Success = false,
                        Message = L("MobileApi.Messages.ConfirmPhoneNumber"),
                        ConfirmationCode = _sendSmsResult.ConfirmationCode,
                        EmailAddress = _sendSmsResult.EmailAddress,
                        UserId = loginResult.User.Id,
                        PhoneNumber = _sendSmsResult.PhoneNumber,
                        PhoneConfirmedError = true
                    });

                case AbpLoginResultType.UserIsNotActive:
                    return Json(new { Success = false, Message = L("Messages.UserIsNotActive", input.PhoneNumber) });

                case AbpLoginResultType.LockedOut:
                    return Json(new { Success = false, Message = L("MobileApi.Messages.UserNotActive") });

                default: //Can not fall to default for now. But other result types can be added in the future and we may forget to handle it
                    return Json(new { Success = false, Message = "Unknown problem with login: " + loginResult.Result });

                    //throw new UserFriendlyException("Unknown problem with login: " + loginResult.Result);
            }

            await SignInAsync(loginResult.User, loginResult.Identity, input.RememberMe);

            if (string.IsNullOrWhiteSpace(input.ReturnUrl))
            {
                input.ReturnUrl = Request.ApplicationPath;
            }

            return Json(new { Success = true, TargetUrl = input.ReturnUrl });
        }


        [HttpPost]
        [DisableAuditing]
        public async Task<ResendOutputModel> ResendConfirmationCode(FrontLoginModel input)
        {
            CheckModelState();

            var user = await _userRepository.FirstOrDefaultAsync(x => x.PhoneNumber == input.PhoneNumber);
            if (user != null)
            {
                user.EmailConfirmationCode = new Random().Next(1000, 9999).ToString();
                await _userManager.UpdateAsync(user);
                await _unitOfWorkManager.Current.SaveChangesAsync();

                //string _msg = L("MobileApi.Messages.CodeMessage", user.EmailConfirmationCode);

                //SMSHelper smsHelper = new SMSHelper();

                //SendMessageInput data = new SendMessageInput
                //{
                //    MessageText = _msg,
                //    PhoneNumbers = "966" + user.PhoneNumber
                //};
                //SendingResult sendResult = smsHelper.SendMessage(data);


                return new ResendOutputModel
                {

                    Success = true,
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    ConfirmationCode = user.EmailConfirmationCode,
                    EmailAddress = user.EmailAddress
                };

            }

            return new ResendOutputModel
            {

                Success = true,
                Message = L("MobileApi.Messages.UserNotfound")
            };
        }


        public JsonResult FrontLogout()
        {
            AuthenticationManager.SignOut();
            return Json(new AjaxResponse { TargetUrl = "/" });
        }

        //public ActionResult FrontLogout()
        //{
        //    AuthenticationManager.SignOut();
        //    return RedirectToAction("Index", "Home");
        //}

        #endregion





        [HttpPost]
        [DisableAuditing]
        public async Task<JsonResult> PublicLogin(PublicLoginModel loginModel, string returnUrl = "", string returnUrlHash = "")
        {
            CheckModelState();
            var user = await _userManager.GetUserByIdAsync(loginModel.Id);

            var loginResult = await _logInManager.LoginAsync(user.EmailAddress, "123");
            ////////////////////////////////////////
            await SignInAsync(loginResult.User, loginResult.Identity, false);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Request.ApplicationPath;
            }

            if (!string.IsNullOrWhiteSpace(returnUrlHash))
            {
                returnUrl += returnUrlHash;
            }

            returnUrl = "/Admin#/dashboard";

            return Json(new { TargetUrl = returnUrl });
        }



        [HttpGet]
        public async Task<JsonResult> MagicLogin(long id)
        {
            var user = await _userManager.GetUserByIdAsync(id);

            var loginResult = await _logInManager.LoginAsync(user.EmailAddress, "123");
            ////////////////////////////////////////
            await SignInAsync(loginResult.User, loginResult.Identity, false);

            return Json(new { TargetUrl = "/" }, JsonRequestBehavior.AllowGet); // Allow GET request
        }


        public class PublicLoginModel
        {
            [Required]
            public long Id { get; set; }
        }

    }
}
