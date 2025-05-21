using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Journals.Dto;

namespace Sayarah.Application.Journals;

public interface IJournalAppService : IAsyncCrudAppService<JournalDto , long, GetAllJournals , CreateJournalDto , UpdateJournalDto>
{
    Task<DataTableOutputDto<JournalDto>> GetPaged(GetJournalsInput input);
    Task<PagedResultDto<ApiJournalDto>> GetAllJournals(GetAllJournals input);
}
