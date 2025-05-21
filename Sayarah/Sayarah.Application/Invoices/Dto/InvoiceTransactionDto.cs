using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Invoices;

namespace Sayarah.Application.Invoices.Dto
{
    [AutoMapFrom(typeof(InvoiceTransaction)), AutoMapTo(typeof(InvoiceTransaction))]
    public class InvoiceTransactionDto : FullAuditedEntityDto<long>
    {
        public long? InvoiceId { get; set; }
        //public InvoiceDto Invoice { get; set; }

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
   

    [AutoMapTo(typeof(InvoiceTransaction))]
    public class CreateInvoiceTransactionDto
    {
        public long? InvoiceId { get; set; }
        public long? VeichleId { get; set; }
        public decimal Price { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes TransType { get; set; }

        public FuelType? TransFuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public decimal Quantity { get; set; } // litre
      
        public int Serial { get; set; }

    }

    [AutoMapTo(typeof(InvoiceTransaction))]
    public class UpdateInvoiceTransactionDto : EntityDto<long>
    {
        public long? InvoiceId { get; set; }
        public long? VeichleId { get; set; }
        public decimal Price { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes TransType { get; set; }
        public FuelType? TransFuelType { get; set; }
        public decimal FuelPrice { get; set; } // سعر لتر الوقود
        public decimal Quantity { get; set; } // litre
        
        public int Serial { get; set; }

    }
    public class GetInvoiceTransactionsInput : DataTableInputDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? VeichleId { get; set; }
        public decimal? Price { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public long? InvoiceId { get; set; }
        public int? Serial { get; set; }
        
        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }

    }
    public class GetAllInvoiceTransactions : PagedResultRequestDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? VeichleId { get; set; }
        public decimal? Price { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public long? InvoiceId { get; set; }
        public int? Serial { get; set; }
      
        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public bool MaxCount { get; set; }
    }
}
