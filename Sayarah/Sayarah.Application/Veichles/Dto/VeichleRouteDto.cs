using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Veichles;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(VeichleRoute)) , AutoMapTo(typeof(VeichleRoute))]
    public class VeichleRouteDto : AuditedEntityDto<long>
    {
        public long? DriverId { get; set; }
        public DriverDto Driver { get; set; }
               
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }
               
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }
               
               
        public string Code { get; set; }
               
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
               
               
        public string Notes { get; set; }
    }
      
    [AutoMapTo(typeof(VeichleRoute))]
    public class CreateVeichleRouteDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }

        public string Code { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public string Notes { get; set; }

    }

 
    [AutoMapTo(typeof(VeichleRoute))]
    public class UpdateVeichleRouteDto : EntityDto<long>
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }

        public string Code { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public string Notes { get; set; }
    }

     
    public class GetVeichleRoutesPagedInput : DataTableInputDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }

        public string DriverName { get; set; }
        public string Code { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public string Notes { get; set; }

        public long? CompanyId { get; set; }
    }
 
  
    public class GetVeichleRoutesInput : PagedResultRequestDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }

        public string Code { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public string Notes { get; set; }

        public long? CompanyId { get; set; }
        public bool MaxCount { get; set; }
      
    }
     
}