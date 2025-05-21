using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.MultiTenancy.Dto;

namespace Sayarah.Application.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}
