using Abp.Application.Services;
using Sayarah.Application.authorization.Accounts.Dto;
using Sayarah.Application.Authorization.Accounts.Dto;

namespace Sayarah.Application.authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);
        Task<RegisterOutput> Register(RegisterInput input);
    }
}
