using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.OilTransactions.Dto;

namespace Sayarah.Application.Transactions.OilTransactions
{
    public interface IOilTransInAppService : IAsyncCrudAppService<OilTransInDto, long, GetOilTransInsInput, CreateOilTransInDto, UpdateOilTransInDto>
    {
        Task<DataTableOutputDto<OilTransInDto>> GetPaged(GetOilTransInsPagedInput input);
    }
}
