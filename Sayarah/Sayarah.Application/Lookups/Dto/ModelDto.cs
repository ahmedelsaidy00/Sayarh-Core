using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Lookups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(Model)), AutoMapTo(typeof(Model))]
    public class ModelDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        //public string CreatorUserName { get; set; }
        //public string LastModifierUserName { get; set; }

        public long? BrandId { get; set; }
        public BrandDto Brand { get; set; }

    }
    [AutoMapFrom(typeof(Model))]
    public class ApiModelDto : EntityDto<long>
    {
        public string Name { get; set; }
        public long? BrandId { get; set; }
    }

    [AutoMapTo(typeof(Model))]
    public class CreateModelDto
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
        public long? BrandId { get; set; }
    }

    [AutoMapTo(typeof(Model))]
    public class UpdateModelDto : EntityDto<long>
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
        public long? BrandId { get; set; }
    }
    public class GetModelsInput : DataTableInputDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool MatchExact { get; set; }
        public long? BrandId { get; set; }
    }
    public class GetAllModels : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }
        public long? BrandId { get; set; }
    }
}
