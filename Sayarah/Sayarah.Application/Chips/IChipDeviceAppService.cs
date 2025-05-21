using Abp.Application.Services;
using Sayarah.Application.Chips.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Chips;

public interface IChipDeviceAppService : IAsyncCrudAppService<ChipDeviceDto , long, GetAllChipDevices , CreateChipDeviceDto , UpdateChipDeviceDto>
{
    Task<DataTableOutputDto<ChipDeviceDto>> GetPaged(GetChipDevicesInput input);
}
