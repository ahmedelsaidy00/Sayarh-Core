using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;

namespace Sayarah.Application.Providers;

public interface IProviderAppService : IAsyncCrudAppService<ProviderDto, long, GetProvidersInput, CreateProviderDto, UpdateProviderDto>
{
    Task<DataTableOutputDto<ProviderDto>> GetPaged(GetProvidersPagedInput input);
    Task<ProviderDto> GetByUserId(EntityDto<long> input);
    Task<PagedResultDto<ApiProviderDto>> GetAllProvidersMobile(GetProvidersInputApi input);
    Task<PagedResultDto<PlainProviderDto>> GetAllLocations(GetProvidersInput input);
}
