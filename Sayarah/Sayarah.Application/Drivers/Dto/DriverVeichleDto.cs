using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Drivers;

namespace Sayarah.Application.Drivers.Dto
{
    [AutoMapFrom(typeof(DriverVeichle)) , AutoMapTo(typeof(DriverVeichle))]
    public class DriverVeichleDto : AuditedEntityDto<long>
    {
        public long? DriverId { get; set; }
        public DriverDto Driver { get; set; }
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }
        public string Serial { get; set; }  
        public bool IsCurrent { get; set; }
    }

    [AutoMapFrom(typeof(DriverVeichle)), AutoMapTo(typeof(DriverVeichle))]
    public class SmallDriverVeichleDto : EntityDto<long>
    {
        public long? DriverId { get; set; }  
        public long? VeichleId { get; set; }  
        public bool IsCurrent { get; set; }
        public ApiVeichleDto Veichle { get; set; }

    }


    [AutoMapTo(typeof(DriverVeichle))]
    public class CreateDriverVeichleDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public string Serial { get; set; }
        public bool IsCurrent { get; set; }
    }

 
    [AutoMapTo(typeof(DriverVeichle))]
    public class UpdateDriverVeichleDto : EntityDto<long>
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public string Serial { get; set; }
        public bool IsCurrent { get; set; }
    }

     
    public class GetDriverVeichlesPagedInput : DataTableInputDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public string Serial { get; set; }
        public string VeichleName { get; set; }
        public bool? IsCurrent { get; set; }
        public long? CityId { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? Id { get; set; }
        public bool? IsActive { get; set; }
    }
 
  
    public class GetDriverVeichlesInput : PagedResultRequestDto
    {
        public long? DriverId { get; set; }
        public long? DriverUserId { get; set; }
        public long? VeichleId { get; set; }
        public string Serial { get; set; }
        public bool? IsCurrent { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public bool? IsActive { get; set; }
        public bool MaxCount { get; set; }
    } 
     
}