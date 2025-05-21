using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.BranchRequests.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.BranchRequests
{
    public interface IBranchRequestAppService : IAsyncCrudAppService<BranchRequestDto, long, GetAllBranchRequest, CreateBranchRequestDto, UpdateBranchRequestDto>
    {
        Task<DataTableOutputDto<BranchRequestDto>> GetPaged(GetBranchRequestInput input);
        Task<BranchRequestDto> Reject(EntityDto<long> input);
    }
}
