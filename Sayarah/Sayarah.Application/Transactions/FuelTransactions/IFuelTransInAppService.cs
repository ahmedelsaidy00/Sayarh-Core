using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;

namespace Sayarah.Application.Transactions.FuelTransactions
{
    public interface IFuelTransInAppService : IAsyncCrudAppService<FuelTransInDto, long, GetFuelTransInsInput, CreateFuelTransInDto, UpdateFuelTransInDto>
    {
        Task<DataTableOutputDto<FuelTransInDto>> GetPaged(GetFuelTransInsPagedInput input);
    }
}
