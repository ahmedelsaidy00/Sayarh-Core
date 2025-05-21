using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Veichles
{
    public interface IVeichleTransferRecordAppService : IAsyncCrudAppService<VeichleTransferRecordDto, long, GetVeichleTransferRecordsInput, CreateVeichleTransferRecordDto, UpdateVeichleTransferRecordDto>
    {
        Task<DataTableOutputDto<VeichleTransferRecordDto>> GetPaged(GetVeichleTransferRecordsPagedInput input);

        Task<bool> ManageTransfers(ManageTransfersInput input);
    }
}
