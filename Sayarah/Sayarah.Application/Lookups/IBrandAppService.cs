using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;

public interface IBrandAppService : IAsyncCrudAppService<BrandDto , long, GetAllBrands , CreateBrandDto , UpdateBrandDto>
{
    Task<DataTableOutputDto<BrandDto>> GetPaged(GetBrandsInput input);
}
