using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Invoices;

namespace Sayarah.Application.Invoices.Dto
{
    [AutoMapFrom(typeof(InvoiceDetail)), AutoMapTo(typeof(InvoiceDetail))]
    public class InvoiceDetailDto : FullAuditedEntityDto<long>
    {
        public long? InvoiceId { get; set; }
       // public InvoiceDto Invoice { get; set; }

        public int Serial { get; set; }
        public long? ItemId { get; set; }
        public AccountTypes AccountType { get; set; }

        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public string Note { get; set; }

    }
   

    [AutoMapTo(typeof(InvoiceDetail))]
    public class CreateInvoiceDetailDto
    {
        public long? InvoiceId { get; set; }
        public int Serial { get; set; }
        public long? ItemId { get; set; }
        public AccountTypes AccountType { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public string Note { get; set; }

    }

    [AutoMapTo(typeof(InvoiceDetail))]
    public class UpdateInvoiceDetailDto : EntityDto<long>
    {
        public long? InvoiceId { get; set; }
        public int Serial { get; set; }
        public long? ItemId { get; set; }
        public AccountTypes AccountType { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public string Note { get; set; }
    }
    public class GetInvoiceDetailsInput : DataTableInputDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public long? InvoiceId { get; set; }
        public int? Serial { get; set; }
        public long? ItemId { get; set; }
        public AccountTypes? AccountType { get; set; }
        public decimal? Price { get; set; }
        public bool? IsTaxable { get; set; }
        public string Note { get; set; }


    }
    public class GetAllInvoiceDetails : PagedResultRequestDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public string ProviderName { get; set; }
        public long? JournalId { get; set; }
        public long? InvoiceId { get; set; }
        public int? Serial { get; set; }
        public long? ItemId { get; set; }
        public AccountTypes? AccountType { get; set; }
        public decimal? Price { get; set; }
        public bool? IsTaxable { get; set; }
        public string Note { get; set; }

        public bool MaxCount { get; set; }
    }

     
}
