using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;

public interface ICityAppService : IAsyncCrudAppService<CityDto , long, GetAllCities , CreateCityDto , UpdateCityDto>
{
    Task<DataTableOutputDto<CityDto>> GetPaged(GetCitiesInput input);
}
