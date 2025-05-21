using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.Sessions.Dto;

namespace Sayarah.Application.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
