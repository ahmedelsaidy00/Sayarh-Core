using Abp.Application.Services;
using Sayarah.Application.BranchRequests.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.BranchRequests
{
    public interface IRequestOrderAppService : IAsyncCrudAppService<RequestOrderDto, long, GetAllRequestOrder, CreateRequestOrderDto, UpdateRequestOrderDto>
    {
        Task<DataTableOutputDto<RequestOrderDto>> GetPaged(GetRequestOrderInput input);
    }
}
