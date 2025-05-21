using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Abp.MultiTenancy;
using Sayarah.MultiTenancy;

namespace Sayarah.Application.MultiTenancy.Dto
{
    [AutoMapTo(typeof(Tenant))]
    public class EditTenantDto
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        [RegularExpression(AbpTenantBase.TenancyNameRegex)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}