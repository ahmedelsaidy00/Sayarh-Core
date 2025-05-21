using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.CompanyInvoices;

namespace Sayarah.Application.CompanyInvoices.Dto
{
    [AutoMapFrom(typeof(CompanyInvoice)), AutoMapTo(typeof(CompanyInvoice))]
    public class CompanyInvoiceDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }
        public virtual long? MainProviderId { get; set; }
        public virtual MainProviderDto MainProvider { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal VatValue { get; set; }
        public decimal AmountWithOutVat { get; set; }
        public decimal Quantity { get; set; }
        public decimal Net { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public ICollection<CompanyInvoiceTransactionDto> CompanyInvoiceTransactions { get; set; }
    }
    [AutoMapFrom(typeof(CompanyInvoice)), AutoMapTo(typeof(CompanyInvoice))]
    public class ShortCompanyInvoiceDto : EntityDto<long>
    {
             public virtual long? MainProviderId { get; set; }

    }

    [AutoMapTo(typeof(CompanyInvoice))]
    public class CreateCompanyInvoiceDto
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }

        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }

    [AutoMapTo(typeof(CompanyInvoice))]
    public class UpdateCompanyInvoiceDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }

        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Net { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }

    }
    public class GetCompanyInvoicesInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; }
        public string Notes { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public decimal? DiscountFrom { get; set; }
        public decimal? DiscountTo { get; set; }
        public decimal? TaxesFrom { get; set; }
        public decimal? TaxesTo { get; set; }
        public decimal? NetFrom { get; set; }
        public decimal? NetTo { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }

    }
    public class GetAllCompanyInvoices : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyInvoiceId { get; set; }
        public long? MainProviderId { get; set; }
        public long? CompanyId { get; set; }
        public string Notes { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Net { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public bool MaxCount { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
    }



    [AutoMapTo(typeof(CompanyInvoice))]
    public class CreateCompanyInvoiceInput
    {
        public CreateCompanyInvoiceDto CompanyInvoice { get; set; }
        public List<TransactionDto> Transactions { get; set; }


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




    public class CompanyInvoiceOutput
    {
        public CompanyInvoiceDto CompanyInvoice { get; set; }
        //public List<FuelOutput> FuelDetails { get; set; }
        public string QrCode { get; set; }
    }

    public class FuelOutput
    {
        //public FuelType FuelType { get; set; }
        public decimal Price { get; set; }
        //public decimal FuelPrice { get; set; }
        public decimal Quantity { get; set; }


        //public decimal PriceWithoutVat
        //{
        //    get
        //    {
        //        return (FuelPrice * Quantity) / 1.15m;
        //    }
        //}

        public decimal PriceWithoutVat
        {
            get
            {
                return Price / 1.15m;
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


    public class CompanyInvoiceWithTransouts
    {
        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }
        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }
        public List<FuelTransOutDto> FuelTransOuts { get; set; }
    }

    public class InvoiceLatestCode
    {
        public string LatestCode { get; set; }
    }

}
