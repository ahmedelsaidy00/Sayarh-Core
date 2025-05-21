using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;

namespace Sayarah.Application.Providers;

public interface IFuelPumpAppService : IAsyncCrudAppService<FuelPumpDto, long, GetFuelPumpsInput, CreateFuelPumpDto, UpdateFuelPumpDto>
{
    Task<DataTableOutputDto<FuelPumpDto>> GetPaged(GetFuelPumpsPagedInput input);
    Task<bool> CreateMultiple(CreateMultipleFuelPumpDto input);
    Task<List<GenerateQrCodeListOutput>> GenerateQrCodeList(GenerateQrCodeList input);
}
