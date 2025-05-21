using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Veichles
{
    public interface IVeichleRouteAppService : IAsyncCrudAppService<VeichleRouteDto, long, GetVeichleRoutesInput, CreateVeichleRouteDto, UpdateVeichleRouteDto>
    {
        Task<DataTableOutputDto<VeichleRouteDto>> GetPaged(GetVeichleRoutesPagedInput input);
    }
}
