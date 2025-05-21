using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(FuelGroup)) , AutoMapTo(typeof(FuelGroup))]
    public class FuelGroupDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }
        public string Code { get; set; }
               
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }
        public GroupType GroupType { get; set; }
               
        public decimal Amount { get; set; }
        public PeriodType? PeriodType { get; set; }
        public bool Transferable { get; set; } // قابل للترحيل

        public decimal LitersCount { get; set; } 
        public decimal MaximumRechargeAmount { get; set; } 
        public PeriodConsumptionType PeriodConsumptionType { get; set; }
        public decimal MaximumRechargeAmountForOnce { get; set; } //أقصى مبلغ للتعبئة للمرة الوحدة
    }
      
    [AutoMapTo(typeof(FuelGroup))]
    public class CreateFuelGroupDto
    {
        public long? BranchId { get; set; }
        public string Code { get; set; }

        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public GroupType GroupType { get; set; }
        public decimal Amount { get; set; }
        public PeriodType? PeriodType { get; set; }
        public bool Transferable { get; set; } // قابل للترحيل
        public decimal LitersCount { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public PeriodConsumptionType PeriodConsumptionType { get; set; }
        public decimal MaximumRechargeAmountForOnce { get; set; } //أقصى مبلغ للتعبئة للمرة الوحدة
    }

 
    [AutoMapTo(typeof(FuelGroup))]
    public class UpdateFuelGroupDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public string Code { get; set; }

        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public GroupType GroupType { get; set; }

        public decimal Amount { get; set; }
        public PeriodType? PeriodType { get; set; }
        public bool Transferable { get; set; } // قابل للترحيل
        public decimal LitersCount { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public PeriodConsumptionType PeriodConsumptionType { get; set; }
        public decimal MaximumRechargeAmountForOnce { get; set; } //أقصى مبلغ للتعبئة للمرة الوحدة
    }

    public class GetFuelGroupsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public GroupType? GroupType { get; set; }
    }
 
  
    public class GetFuelGroupsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public GroupType? GroupType { get; set; }

        public bool MaxCount { get; set; }

    }
}