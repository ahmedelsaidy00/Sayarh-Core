using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Journals.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Journals
{
    public interface IJournalDetailAppService : IAsyncCrudAppService<JournalDetailDto , long, GetAllJournalDetails , CreateJournalDetailDto , UpdateJournalDetailDto>
    {
        Task<DataTableOutputDto<JournalDetailDto>> GetPaged(GetJournalDetailsInput input);
    }
}
