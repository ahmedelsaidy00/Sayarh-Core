
namespace Sayarah.Web.Models.Account
{
    public class ConfirmEmailViewModel
    {
        public string TenancyName { get; set; }
        public string EmailAddress { get; set; }
        public bool Success { get; set; }
        public string DeviceType { get; set; }

    }
}