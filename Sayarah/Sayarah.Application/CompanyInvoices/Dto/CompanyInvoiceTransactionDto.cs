using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.CompanyInvoices;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.CompanyInvoices.Dto
{
    [AutoMapFrom(typeof(CompanyInvoiceTransaction)), AutoMapTo(typeof(CompanyInvoiceTransaction))]
    public class CompanyInvoiceTransactionDto : FullAuditedEntityDto<long>
    {
        public long? CompanyInvoiceId { get; set; }  
        public ShortCompanyInvoiceDto CompanyInvoice { get; set; }  
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }

        public decimal Price { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes TransType { get; set; }

        public FuelType? FuelType { get; set; }
        public FuelType? TransFuelType { get; set; }
        public decimal Quantity { get; set; } // litre
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public int Serial { get; set; }   
    }
   

    [AutoMapTo(typeof(CompanyInvoiceTransaction))]
    public class CreateCompanyInvoiceTransactionDto
    {
        public long? CompanyInvoiceId { get; set; }
        public long? VeichleId { get; set; }
        public decimal Price { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes TransType { get; set; }

        public FuelType? TransFuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public decimal Quantity { get; set; } // litre
      
        public int Serial { get; set; }

    }

    [AutoMapTo(typeof(CompanyInvoiceTransaction))]
    public class UpdateCompanyInvoiceTransactionDto : EntityDto<long>
    {
        public long? CompanyInvoiceId { get; set; }
        public long? VeichleId { get; set; }
        public decimal Price { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes TransType { get; set; }
        public FuelType? TransFuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public decimal Quantity { get; set; } // litre
        
        public int Serial { get; set; }

    }
    public class GetCompanyInvoiceTransactionsInput : DataTableInputDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? VeichleId { get; set; }
        public decimal? Price { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public long? CompanyInvoiceId { get; set; }
        public int? Serial { get; set; }
        
        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }

    }
    public class GetAllCompanyInvoiceTransactions : PagedResultRequestDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? VeichleId { get; set; }
        public decimal? Price { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public long? CompanyInvoiceId { get; set; }
        public int? Serial { get; set; }
      
        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public bool MaxCount { get; set; }
    }
}
