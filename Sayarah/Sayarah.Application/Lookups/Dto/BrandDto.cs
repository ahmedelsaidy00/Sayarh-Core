using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Lookups;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(Brand)), AutoMapTo(typeof(Brand))]
    public class BrandDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
    [AutoMapFrom(typeof(Brand))]
    public class ApiBrandDto : EntityDto<long>
    {
        public string Name { get; set; }
    }

    [AutoMapTo(typeof(Brand))]
    public class CreateBrandDto
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }

    [AutoMapTo(typeof(Brand))]
    public class UpdateBrandDto : EntityDto<long>
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }
    public class GetBrandsInput : DataTableInputDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool MatchExact { get; set; }

    }
    public class GetAllBrands : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }
    }
}
