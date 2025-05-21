using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.OilTransactions.Dto;

namespace Sayarah.Application.Transactions.OilTransactions
{
    public interface IOilTransOutAppService : IAsyncCrudAppService<OilTransOutDto, long, GetOilTransOutsInput, CreateOilTransOutDto, UpdateOilTransOutDto>
    {
        Task<DataTableOutputDto<OilTransOutDto>> GetPaged(GetOilTransOutsPagedInput input);
    }
}
