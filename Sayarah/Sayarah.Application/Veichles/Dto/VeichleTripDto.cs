using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Veichles;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(VeichleTrip)) , AutoMapTo(typeof(VeichleTrip))]
    public class VeichleTripDto : AuditedEntityDto<long>
    {
        public long? VeichleId { get; set; }
        public ApiVeichleDto Veichle { get; set; }
        public long? BranchId { get; set; }
        public ApiBranchDto Branch { get; set; }
        public string Code { get; set; }
        public string TripNumber { get; set; }
        public decimal MaxLitersCount { get; set; } // أقصى استهلاك للرحلة بالليتر
        public decimal ConsumptionBySar { get; set; } // Trip consumption by SAR
        public decimal CurrentConsumption { get; set; } // الاستهلاك الحالي
        public bool IsActive { get; set; }
               
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
               
        public string Notes { get; set; }
        public string CreatorUserName { get; set; }
    }
      
    [AutoMapTo(typeof(VeichleTrip))]
    public class CreateVeichleTripDto
    {
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }
        public string Code { get; set; }
        public string TripNumber { get; set; }
        public decimal MaxLitersCount { get; set; } // أقصى استهلاك للرحلة بالليتر
        public decimal ConsumptionBySar { get; set; } // Trip consumption by SAR
        public decimal CurrentConsumption { get; set; } // الاستهلاك الحالي
        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Notes { get; set; }

    }

 
    [AutoMapTo(typeof(VeichleTrip))]
    public class UpdateVeichleTripDto : EntityDto<long>
    {
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }
        public string Code { get; set; }
        public string TripNumber { get; set; }
        public decimal MaxLitersCount { get; set; } // أقصى استهلاك للرحلة بالليتر
        public decimal ConsumptionBySar { get; set; } // Trip consumption by SAR
        public decimal CurrentConsumption { get; set; } // الاستهلاك الحالي
        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Notes { get; set; }
    }


    [AutoMapTo(typeof(VeichleTrip)) , AutoMapFrom(typeof(VeichleTrip))]
    public class ManageVeichleTripDto 
    {
        public long? Id { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }
        public string Code { get; set; }
        public string TripNumber { get; set; }
        public decimal? MaxLitersCount { get; set; } // أقصى استهلاك للرحلة بالليتر
        public decimal? ConsumptionBySar { get; set; } // Trip consumption by SAR
        public decimal? CurrentConsumption { get; set; } // الاستهلاك الحالي
        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Notes { get; set; }
    }

    public class GetVeichleTripsPagedInput : DataTableInputDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }
         
        public string Code { get; set; }
        public string VeichleName { get; set; }
        public string BranchName { get; set; }
        public string TripNumber { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }


        public long? CompanyId { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }
 
  
    public class GetVeichleTripsInput : PagedResultRequestDto
    {
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public long? BranchId { get; set; }

        public string Code { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public string Notes { get; set; }

      
        public bool MaxCount { get; set; }
      
    }


    public class RequestTripsExcelDtoInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
      
      
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
      
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
    
        public int SkipCount { get; set; }
        public int MaxResultCount { get; set; }

         
        public string Code { get; set; }
        public string VeichleName { get; set; }
        public string BranchName { get; set; }
        public string TripNumber { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }

 
        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

    }



    public class RequestTripExcelCompanyDto
    {
        public string ExcelTitle { get; set; }
        public string Code { get; set; }    
        public string Branch { get; set; }
        public string Veichle { get; set; }
        public string FuelType { get; set; }
        public string MaxLitersCount { get; set; }
        public string CurrentConsumption { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CreationTime { get; set; }

    }


}