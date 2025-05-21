using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;

namespace Sayarah.Application.Drivers;

public interface IDriverAppService : IAsyncCrudAppService<DriverDto, long, GetDriversInput, CreateDriverDto, UpdateDriverDto>
{
    Task<DataTableOutputDto<DriverDto>> GetPaged(GetDriversPagedInput input);
    Task<DriverDto> GetByUserId(EntityDto<long> input);
    Task<DriverDto> UpdateMobile(UpdateDriverProfileInput input);
    Task<DriverDto> UpdateDriverPhotoAsync(UpdateDriverDto input);
    Task<string> ExportExcel(RequestDriversExcelDtoInput input);
}
