using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Companies;

public interface IBranchAppService : IAsyncCrudAppService<BranchDto, long, GetBranchesInput, CreateBranchDto, UpdateBranchDto>
{
    Task<DataTableOutputDto<BranchDto>> GetPaged(GetBranchesPagedInput input);
    Task<BranchDto> GetByUserId(EntityDto<long> input);
    Task<GetBranchWalletDetailsDto> GetWalletDetails(GetBranchesInput input);
    Task<ManageActiveOutput> ManageActive(EntityDto<long> input);
    Task<BranchDto> UpdateReservedAndBalance(UpdateReservedBalanceBranchDto input);
    Task<List<BranchNameDto>> GetAllBranchs(GetBranchesInput input);
}
