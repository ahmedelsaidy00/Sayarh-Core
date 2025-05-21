using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;

namespace Sayarah.Application.Providers;

public interface IWorkerAppService : IAsyncCrudAppService<WorkerDto, long, GetWorkersInput, CreateWorkerDto, UpdateWorkerDto>
{
    Task<DataTableOutputDto<WorkerDto>> GetPaged(GetWorkersPagedInput input);
    Task<WorkerDto> GetByUserId(EntityDto<long> input);
    Task<WorkerDto> UpdateWorkerPhotoAsync(UpdateWorkerDto input);
    Task<WorkerDto> UpdateMobile(UpdateWorkerProfileInput input);
}
