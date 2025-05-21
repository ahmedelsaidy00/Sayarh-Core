using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;

namespace Sayarah.Application.Drivers;

public interface IDriverVeichleAppService : IAsyncCrudAppService<DriverVeichleDto, long, GetDriverVeichlesInput, CreateDriverVeichleDto, UpdateDriverVeichleDto>
{
    Task<DataTableOutputDto<DriverVeichleDto>> GetPaged(GetDriverVeichlesPagedInput input);
    Task<bool> UpdateIsCurrent(EntityDto<long> input);
}
