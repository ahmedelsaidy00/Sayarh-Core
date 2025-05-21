using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.WashTransactions.Dto;

namespace Sayarah.Application.Transactions.WashTransactions
{
    public interface IWashTransOutAppService : IAsyncCrudAppService<WashTransOutDto, long, GetWashTransOutsInput, CreateWashTransOutDto, UpdateWashTransOutDto>
    {
        Task<DataTableOutputDto<WashTransOutDto>> GetPaged(GetWashTransOutsPagedInput input);
    }
}
