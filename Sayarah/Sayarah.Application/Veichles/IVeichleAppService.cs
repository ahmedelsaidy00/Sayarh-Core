using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System.Collections.Generic;
using Sayarah.Application.Helpers.BackgroundJobs;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Veichles
{
    public interface IVeichleAppService : IAsyncCrudAppService<VeichleDto, long, GetVeichlesInput, CreateVeichleDto, UpdateVeichleDto>
    {
        Task<DataTableOutputDto<VeichleDto>> GetPaged(GetVeichlesPagedInput input);
        Task CheckVeichleExists(UpdateVeichleSimPicDto input);
        Task<VeichleDto> UpdateVeichleDetails(UpdateVeichleSimPicDto input);
        Task<VeichleDto> GetByPlateNum(UpdateVeichleSimPicDto input);

        Task<GetVeichleBySimOutput> GetBySim(GetVeichlesInput input);
        Task<GetVeichleBySimOutput> GetVeichle(GetVeichlesInput input);

        Task<UpdateDriverCodeOutput> ConfirmDriverCode(UpdateDriverCodeInput input);

        Task<bool> HandleEndFuelGroupPeriod(EndFuelGroupPeriodScheduleJobArgs input);

        Task<PagedResultDto<VeichleNumbersDto>> GetAllVeichlesNumbers(GetVeichlesInput input);

        Task<List<ShortVeichleDto>> GetVeichlesListByIds(GetListByIdsInput input);
        Task<List<ImportExcelDataOutput>> ImportExcel(ImportExcelDataInput input);

        Task CreateBackGroundJobs(CreateVeichleDto input);
    }
}
