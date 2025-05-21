using Abp.Application.Services;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;

public interface ICompanyTypeAppService : IAsyncCrudAppService<CompanyTypeDto , long, GetAllCompanyTypes , CreateCompanyTypeDto , UpdateCompanyTypeDto>
{
    Task<DataTableOutputDto<CompanyTypeDto>> GetPaged(GetCompanyTypesInput input);
}
