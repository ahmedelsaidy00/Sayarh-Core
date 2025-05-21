using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Journals;

namespace Sayarah.Application.Journals.Dto
{
    [AutoMapFrom(typeof(Journal)), AutoMapTo(typeof(Journal))]
    public class JournalDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }
               
        public string Notes { get; set; }
               
        public JournalType JournalType { get; set; }

        public List<JournalDetailDto> JournalDetails { get; set; }

    }


    [AutoMapFrom(typeof(Journal)), AutoMapTo(typeof(Journal))]
    public class ApiJournalDto : CreationAuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }

        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public string ProviderName { get; set; }
        public string MainProviderName { get; set; }

        public string Notes { get; set; }

        public JournalType JournalType { get; set; }

    }



    [AutoMapTo(typeof(Journal))]
    public class CreateJournalDto
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; } 

        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public JournalType JournalType { get; set; }

    }

    [AutoMapTo(typeof(Journal))]
    public class UpdateJournalDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; } 

        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public JournalType JournalType { get; set; }
    }
    public class GetJournalsInput : DataTableInputDto
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; } 
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public JournalType? JournalType { get; set; }

    }
    public class GetAllJournals : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public string Notes { get; set; }
        public JournalType? JournalType { get; set; }
        public bool MaxCount { get; set; }
    }
}
