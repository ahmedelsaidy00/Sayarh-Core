using Abp.Application.Services;
using Sayarah.Application.Chips.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Chips;
public interface IChipNumberAppService : IAsyncCrudAppService<ChipNumberDto , long, GetAllChipNumbers , CreateChipNumberDto , UpdateChipNumberDto>
{
    Task<DataTableOutputDto<ChipNumberDto>> GetPaged(GetChipNumbersInput input);
    Task<ChipNumberDto> LinkWithCompany(UpdateChipNumberDto input);
    Task<ChipNumberDto> LinkWithVeichle(UpdateChipNumberDto input);
    Task<LinkByChipsEmployeeOutput> LinkByChipsEmployee(LinkByChipsEmployee input);
}
