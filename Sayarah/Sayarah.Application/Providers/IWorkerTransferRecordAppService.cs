using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;

namespace Sayarah.Application.Providers;

public interface IWorkerTransferRecordAppService : IAsyncCrudAppService<WorkerTransferRecordDto, long, GetWorkerTransferRecordsInput, CreateWorkerTransferRecordDto, UpdateWorkerTransferRecordDto>
{
    Task<DataTableOutputDto<WorkerTransferRecordDto>> GetPaged(GetWorkerTransferRecordsPagedInput input);
    Task<bool> ManageTransfers(ManageTransfersInput input);
}
