using System.Threading.Tasks;
using Abp.Application.Services;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Veichles
{
    public interface IFuelGroupAppService : IAsyncCrudAppService<FuelGroupDto, long, GetFuelGroupsInput, CreateFuelGroupDto, UpdateFuelGroupDto>
    {
        Task<DataTableOutputDto<FuelGroupDto>> GetPaged(GetFuelGroupsPagedInput input);
    }
}
