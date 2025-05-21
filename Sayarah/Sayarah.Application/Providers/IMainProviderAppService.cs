using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users.Dto;

namespace Sayarah.Application.Providers;

public interface IMainProviderAppService : IAsyncCrudAppService<MainProviderDto, long, GetMainProvidersInput, CreateMainProviderDto, UpdateMainProviderDto>
{
    Task<DataTableOutputDto<MainProviderDto>> GetPaged(GetMainProvidersPagedInput input);
    Task<DataTableOutputDto<MainProviderBankInfoDto>> GetBankInfoPaged(GetMainProvidersPagedInput input);
    Task<MainProviderDto> GetByUserId(EntityDto<long> input);
    Task<UserDto> HandleEmailAddress(ChangeEmailAndPhone input);
    Task<ChangeEmailAndPhone> HandleConfirmEmailAddress(ChangeEmailAndPhone input);
    Task<UserDto> HandlePhoneNumber(ChangeEmailAndPhone input);
    Task<ChangeEmailAndPhone> HandleConfirmPhoneNumber(ChangeEmailAndPhone input);
    Task<MainProviderDto> UpdateDocuments(UpdateMainProviderDocumentsDto input);
    Task<MainProviderDto> UpdateBankInfo(UpdateMainProviderBankInfoDto input);
    Task<MainProviderDto> UpdateNationalAddress(UpdateMainProviderNationalAddressDto input);
    Task<string> ExportBankInfoExcel(GetMainProvidersExcelInput input);
}
