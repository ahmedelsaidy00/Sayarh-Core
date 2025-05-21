using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Lookups;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(CompanyType)), AutoMapTo(typeof(CompanyType))]
    public class CompanyTypeDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        //public string CreatorUserName { get; set; }
        //public string LastModifierUserName { get; set; }

    }
    [AutoMapFrom(typeof(CompanyType))]
    public class ApiCompanyTypeDto : EntityDto<long>
    {
        public string Name { get; set; }
    }

    [AutoMapTo(typeof(CompanyType))]
    public class CreateCompanyTypeDto
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }

    [AutoMapTo(typeof(CompanyType))]
    public class UpdateCompanyTypeDto : EntityDto<long>
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }
    public class GetCompanyTypesInput : DataTableInputDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool MatchExact { get; set; }

    }
    public class GetAllCompanyTypes : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }
    }
}
