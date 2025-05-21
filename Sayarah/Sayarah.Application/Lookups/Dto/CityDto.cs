using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Lookups;
using System.ComponentModel.DataAnnotations;

namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(City)), AutoMapTo(typeof(City))]
    public class CityDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
    [AutoMapFrom(typeof(City))]
    public class ApiCityDto : EntityDto<long>
    {
        public string Name { get; set; }
    }

    [AutoMapTo(typeof(City))]
    public class CreateCityDto
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }

    [AutoMapTo(typeof(City))]
    public class UpdateCityDto : EntityDto<long>
    {
        [Required]
        [StringLength(50)]
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
    }
    public class GetCitiesInput : DataTableInputDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool MatchExact { get; set; }

    }
    public class GetAllCities : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }
    }
}
