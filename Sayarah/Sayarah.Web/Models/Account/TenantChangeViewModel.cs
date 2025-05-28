using Abp.AutoMapper;
using Sayarah.Application.Sessions.Dto;

namespace Sayarah.Web.Models.Account
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}