using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Journals.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Invoices;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Invoices.Dto
{
    [AutoMapFrom(typeof(Voucher)), AutoMapTo(typeof(Voucher))]
    public class VoucherDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
               
        public long? JournalId { get; set; }
        public JournalDto Journal { get; set; }
               
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string FilePath { get; set; }

        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(18, FilePath))
                    return FilesPath.Vouchers.ServerImagePath + FilePath;
                else
                    return FilesPath.Vouchers.DefaultImagePath;
            }
        }
    }
   

    [AutoMapTo(typeof(Voucher))]
    public class CreateVoucherDto
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; } 

        public long? ProviderId { get; set; }
        public long? JournalId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string FilePath { get; set; }
    }

    [AutoMapTo(typeof(Voucher))]
    public class UpdateVoucherDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? JournalId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string FilePath { get; set; }
    }
    public class GetVouchersInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? MainProviderId { get; set; } 
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public string MainProviderName { get; set; }
        public string ProviderName { get; set; }
        public long? Id { get; set; }
        public long? JournalId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public bool? IsProviderEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
        public string Note { get; set; }
        public string FilePath { get; set; }
    }
    public class GetAllVouchers : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public long? JournalId { get; set; }
        public decimal? Amount { get; set; }

        public bool MaxCount { get; set; }
        public bool? IsEmployee { get; set; }
        public long[] ProviderIds { get; set; }
    }

    public class GetTotalVouchersAmountOutput  
    {
        public decimal Amount { get; set; }
    }
}
