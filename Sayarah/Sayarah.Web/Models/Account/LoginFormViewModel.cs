using Abp.MultiTenancy;
using Sayarah.Core.Helpers;

namespace Sayarah.Web.Models.Account
{
    public class LoginFormViewModel
    {
        public string ReturnUrl { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public bool IsSelfRegistrationAllowed { get; set; }

        public MultiTenancySides MultiTenancySide { get; set; }
    }


    public class ConfirmConfirmationCodeModel
    {
        public string TenancyName { get; set; }

        public string PhoneNumber { get; set; }
        public string ConfirmationCode { get; set; }
        public string ReturnUrl { get; set; }
    }


    public class FrontLoginModel
    {
        public string TenancyName { get; set; }

        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ResendOutputModel
    {
        public string AccessToken { get; set; }
        public long UserId { get; set; }
        public string ConfirmationCode { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public int UnReadCount { get; set; }
        public UserTypes UserType { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
        public string AvatarPath { get; set; }

        public bool PhoneConfirmedError { get; set; }
    }

}