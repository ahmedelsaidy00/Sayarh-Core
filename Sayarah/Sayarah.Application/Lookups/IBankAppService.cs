using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;

public interface IBankAppService : IAsyncCrudAppService<BankDto , long, GetAllBanks , CreateBankDto , UpdateBankDto>
{
    Task<DataTableOutputDto<BankDto>> GetPaged(GetBanksInput input);
}
