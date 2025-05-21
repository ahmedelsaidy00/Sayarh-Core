using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Transactions;

namespace Sayarah.Application.Transactions.FuelTransactions.Dto
{
    [AutoMapFrom(typeof(FuelTransIn)) , AutoMapTo(typeof(FuelTransIn))]
    public class FuelTransInDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }

        public string Code { get; set; }
        public decimal Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
      
    [AutoMapTo(typeof(FuelTransIn))]
    public class CreateFuelTransInDto
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public decimal Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

 
    [AutoMapTo(typeof(FuelTransIn))]
    public class UpdateFuelTransInDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public decimal Quantity { get; set; } // litre
        public string Notes { get; set; }
    }

     
    public class GetFuelTransInsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public decimal? Quantity { get; set; } // litre
        public string Notes { get; set; }
    }
 
  
    public class GetFuelTransInsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public decimal? Quantity { get; set; } // litre
        public string Notes { get; set; }
        public bool MaxCount { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string PeriodFromString { get; set; }
        public string PeriodToString { get; set; }
        public string FullPeriodFromString { get; set; }
        public string FullPeriodToString { get; set; }
        public string Date { get; set; }
        public int? TransactionType { get; set; }
        public FuelType? FuelType { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? WorkerId { get; set; }
        public bool? IsProviderEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
        public bool? NewTransactions { get; set; }
        public bool? Paginated { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string VeichleName { get; set; }
        public string DriverName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public string WorkerName { get; set; }
        public decimal? Price { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public string InvoiceCode { get; set; } // رقم الفاتورة
        public bool? InvoiceStatus { get; set; }
        public long? BrandId { get; set; }
        public long? ModelId { get; set; }
        public bool? IsCompanyEmployee { get; set; }
    }



}