using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.MaintainTransactions.Dto;

namespace Sayarah.Application.Transactions.MaintainTransactions
{
    public interface IMaintainTransOutAppService : IAsyncCrudAppService<MaintainTransOutDto, long, GetMaintainTransOutsInput, CreateMaintainTransOutDto, UpdateMaintainTransOutDto>
    {
        Task<DataTableOutputDto<MaintainTransOutDto>> GetPaged(GetMaintainTransOutsPagedInput input);
    }
}
