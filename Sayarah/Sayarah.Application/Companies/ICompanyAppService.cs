using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Users.Dto;

namespace Sayarah.Application.Companies;

public interface ICompanyAppService : IAsyncCrudAppService<CompanyDto, long, GetCompaniesInput, CreateCompanyDto, UpdateCompanyDto>
{
    Task<DataTableOutputDto<CompanyDto>> GetPaged(GetCompaniesPagedInput input);
    Task<CompanyDto> GetByUserId(EntityDto<long> input);
    Task<GetWalletDetailsDto> GetWalletDetails(EntityDto<long> input);
    Task<UserDto> HandleEmailAddress(ChangeEmailAndPhone input);
    Task<ChangeEmailAndPhone> HandleConfirmEmailAddress(ChangeEmailAndPhone input);
    Task<UserDto> HandlePhoneNumber(ChangeEmailAndPhone input);
    Task<ChangeEmailAndPhone> HandleConfirmPhoneNumber(ChangeEmailAndPhone input);
    Task<List<CompanyNameDto>> GetAllCompanies(GetCompaniesInput input);

    Task<List<CompanyNameDto>> GetAllCompaniesAdminEmployee();
    Task<List<CompanyNameDto>> GetByUserIdForCompanyUser(EntityDto<long> input);
}
