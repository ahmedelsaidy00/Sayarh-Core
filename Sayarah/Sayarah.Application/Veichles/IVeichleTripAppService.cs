using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Veichles.Dto;

namespace Sayarah.Application.Veichles
{
    public interface IVeichleTripAppService : IAsyncCrudAppService<VeichleTripDto, long, GetVeichleTripsInput, CreateVeichleTripDto, UpdateVeichleTripDto>
    {
        Task<DataTableOutputDto<VeichleTripDto>> GetPaged(GetVeichleTripsPagedInput input);

        Task<string> ExportExcel(RequestTripsExcelDtoInput input);
    }
}
