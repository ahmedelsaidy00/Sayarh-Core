using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Journals;

namespace Sayarah.Application.Journals.Dto
{
    [AutoMapFrom(typeof(JournalDetail)), AutoMapTo(typeof(JournalDetail))]
    public class JournalDetailDto : CreationAuditedEntityDto<long>
    {
        public long? JournalId { get; set; }
        public long? AccountId { get; set; }
        public AccountTypes AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }

    }
   

    [AutoMapTo(typeof(JournalDetail))]
    public class CreateJournalDetailDto
    {
        public long? JournalId { get; set; }
        public long? AccountId { get; set; }
        public AccountTypes AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }
    }

    [AutoMapTo(typeof(JournalDetail))]
    public class UpdateJournalDetailDto : EntityDto<long>
    {
        public long? JournalId { get; set; }
        public long? AccountId { get; set; }
        public AccountTypes AccountType { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }
    }
    public class GetJournalDetailsInput : DataTableInputDto
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }

        public long? JournalId { get; set; }
        public long? AccountId { get; set; }
        public AccountTypes? AccountType { get; set; }
        public decimal? Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }
         

    }
    public class GetAllJournalDetails : PagedResultRequestDto
    {

        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }

        public long? JournalId { get; set; }
        public long? AccountId { get; set; }
        public AccountTypes? AccountType { get; set; }
        public decimal? Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }

        public bool MaxCount { get; set; }
    }
}
