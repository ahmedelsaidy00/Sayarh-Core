using System;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using Abp.UI;
using Microsoft.Extensions.Logging;
using Sayarah.Authorization.Users;
using Sayarah.Authorization;
using Sayarah.MultiTenancy;
using Sayarah.WebApi.Api.Models;

namespace Sayarah.WebApi.Api.Controllers
{
    //[Route("api/[controller]/[action]")]
    [ApiController]

    public class AccountController : AbpController
    {
        private readonly LogInManager _logInManager;

        // You can inject settings or configs for JWT here
        private readonly string _jwtSecret = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1"; // Store in config securely!
        private readonly int _jwtExpirationDays = 3;

        public AccountController(LogInManager logInManager)
        {
            _logInManager = logInManager;
            LocalizationSourceName = SayarahConsts.LocalizationSourceName; // If you use localization
        }

        [HttpPost]
        public async Task<AjaxResponse> Authenticate([FromBody] LoginModel loginModel)
        {
            CheckModelState();

            var loginResult = await GetLoginResultAsync(
                loginModel.UsernameOrEmailAddress,
                loginModel.Password,
                loginModel.TenancyName
            );

            // Create JWT token manually using Microsoft.IdentityModel.Tokens
            var token = CreateJwtToken(loginResult.Identity);

            return new AjaxResponse(token);
        }

        private string CreateJwtToken(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                IssuedAt = now,
                NotBefore = now,
                Expires = now.AddDays(_jwtExpirationDays),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "Sayarah", // You can set your issuer here
                Audience = "Sayarah", // You can set your audience here
                // You can add Issuer and Audience here if needed
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
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

        private Exception CreateExceptionForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    return new ApplicationException("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return new UserFriendlyException(L("LoginFailed"), L("InvalidUserNameOrPassword"));
                case AbpLoginResultType.InvalidTenancyName:
                    return new UserFriendlyException(L("LoginFailed"), L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                case AbpLoginResultType.TenantIsNotActive:
                    return new UserFriendlyException(L("LoginFailed"), L("TenantIsNotActive", tenancyName));
                case AbpLoginResultType.UserIsNotActive:
                    return new UserFriendlyException(L("LoginFailed"), L("UserIsNotActiveAndCanNotLogin", usernameOrEmailAddress));
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return new UserFriendlyException(L("LoginFailed"), "Your email address is not confirmed. You can not login"); //TODO: localize message
                default: //Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return new UserFriendlyException(L("LoginFailed"));
            }
        }

        // Remove the 'override' keyword since there is no base method to override
        protected void CheckModelState()
        {
            if (!ModelState.IsValid)
            {
                throw new UserFriendlyException("Invalid request!");
            }
        }
    }
}
