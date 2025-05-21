using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;

namespace Sayarah.Application.Providers;

public interface IFuelPriceChangeRequestAppService : IAsyncCrudAppService<FuelPriceChangeRequestDto, long, GetFuelPriceChangeRequestsInput, CreateFuelPriceChangeRequestDto, UpdateFuelPriceChangeRequestDto>
{
    Task<DataTableOutputDto<FuelPriceChangeRequestDto>> GetPaged(GetFuelPriceChangeRequestsPagedInput input);
}
