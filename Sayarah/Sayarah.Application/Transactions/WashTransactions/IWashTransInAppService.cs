using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.WashTransactions.Dto;

namespace Sayarah.Application.Transactions.WashTransactions
{
    public interface IWashTransInAppService : IAsyncCrudAppService<WashTransInDto, long, GetWashTransInsInput, CreateWashTransInDto, UpdateWashTransInDto>
    {
        Task<DataTableOutputDto<WashTransInDto>> GetPaged(GetWashTransInsPagedInput input);
    }
}
