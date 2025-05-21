using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;

public interface IModelAppService : IAsyncCrudAppService<ModelDto , long, GetAllModels , CreateModelDto , UpdateModelDto>
{
    Task<DataTableOutputDto<ModelDto>> GetPaged(GetModelsInput input);
}
