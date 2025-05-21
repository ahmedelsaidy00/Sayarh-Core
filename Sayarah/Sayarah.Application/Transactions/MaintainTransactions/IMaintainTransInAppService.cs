using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.MaintainTransactions.Dto;

namespace Sayarah.Application.Transactions.MaintainTransactions
{
    public interface IMaintainTransInAppService : IAsyncCrudAppService<MaintainTransInDto, long, GetMaintainTransInsInput, CreateMaintainTransInDto, UpdateMaintainTransInDto>
    {
        Task<DataTableOutputDto<MaintainTransInDto>> GetPaged(GetMaintainTransInsPagedInput input);
    }
}
