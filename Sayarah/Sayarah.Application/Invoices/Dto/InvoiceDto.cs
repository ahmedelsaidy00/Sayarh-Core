using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Journals.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Invoices;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Invoices.Dto
{
    [AutoMapFrom(typeof(Invoice)), AutoMapTo(typeof(Invoice))]
    public class InvoiceDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
               
        public long? JournalId { get; set; }
        public JournalDto Journal { get; set; }
               
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }

        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }


        public ICollection<InvoiceTransactionDto> InvoiceTransactions { get; set; }
        public ICollection<InvoiceDetailDto> InvoiceDetails { get; set; }

        public ICollection<BranchInvoiceDetailDto> BranchInvoiceDetails { get; set; }



        public bool ExternalInvoice { get; set; }
        public string ExternalCode { get; set; }
        public string FilePath { get; set; }

        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(19,  FilePath))
                    return FilesPath.Invoices.ServerImagePath + FilePath;
                else
                    return "";
            }
        }
    }


    public class BranchInvoiceDetailDto 
    {
        public long? MainProviderId { get; set; }
        public string BranchName { get; set; }
        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal Price { get; set; }
    }


    [AutoMapTo(typeof(Invoice))]
    public class CreateInvoiceDto
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }

        public long? ProviderId { get; set; }

        public long? JournalId { get; set; }

        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }

        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }

        public bool ExternalInvoice { get; set; }
        public string ExternalCode { get; set; }
        public string FilePath { get; set; }

    }

    [AutoMapTo(typeof(Invoice))]
    public class UpdateInvoiceDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }

        public long? ProviderId { get; set; }

        public long? JournalId { get; set; }

        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }

        public bool ExternalInvoice { get; set; }
        public string ExternalCode { get; set; }
        public string FilePath { get; set; }

    }
    public class GetInvoicesInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
       
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public long? JournalId { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public decimal? DiscountFrom { get; set; }
        public decimal? DiscountTo { get; set; }
        public decimal? TaxesFrom { get; set; }
        public decimal? TaxesTo { get; set; }
        public decimal? NetFrom { get; set; }
        public decimal? NetTo { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public bool? IsProviderEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

        public bool? ExternalInvoice { get; set; }
        public string ExternalCode { get; set; }

    }
    public class GetAllInvoices : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
      
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Net { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public bool MaxCount { get; set; }

        public bool? ExternalInvoice { get; set; }
        public string ExternalCode { get; set; }
        public string FilePath { get; set; }

    }



    [AutoMapTo(typeof(Invoice))]
    public class CreateInvoiceInput
    {
        public CreateInvoiceDto Invoice { get; set; }
        public List<TransactionDto> Transactions { get; set; }
        public List<CreateInvoiceDetailDto> InvoiceDetails { get; set; }


        // New property to concatenate Transaction Ids with commas
        public string TransactionIds
        {
            get
            {
                // Check if Transactions is not null and has elements
                if (Transactions != null && Transactions.Any())
                {
                    // Concatenate the Ids into a comma-separated string
                    return string.Join(",", Transactions.Select(t => t.Id));
                }
                return string.Empty; // Return an empty string if Transactions is null or empty
            }
        }

    }




    public class InvoiceOutput
    {
        public InvoiceDto Invoice { get; set; }
        public List<FuelOutput> FuelDetails { get; set; }
        public string QrCode { get; set; }
    }

    public class FuelOutput
    {
        public FuelType FuelType { get; set; }
        public decimal Price { get; set; }
        public decimal FuelPrice { get; set; }
        public decimal Quantity { get; set; }


        public decimal PriceWithoutVat {
            get {
                return FuelPrice * Quantity;
            }
        }


        public decimal VatValue
        {
            get
            {
                return PriceWithoutVat * 15 / 100;
            }
        }


        public decimal TotalPrice
        {
            get
            {
 
                return VatValue + PriceWithoutVat;
            }
        }


    }

}
