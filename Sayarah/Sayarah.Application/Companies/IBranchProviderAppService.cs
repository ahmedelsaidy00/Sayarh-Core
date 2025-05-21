using Abp.Application.Services;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Companies;

public interface IBranchProviderAppService : IAsyncCrudAppService<BranchProviderDto, long, GetBranchProviderProvidersInput, CreateBranchProviderDto, UpdateBranchProviderDto>
{
    Task<DataTableOutputDto<BranchProviderDto>> GetPaged(GetBranchProviderProvidersPagedInput input);
}