using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Lookups.Dto;

namespace Sayarah.Application.Lookups;
public interface IAnnouncementAppService : IAsyncCrudAppService<AnnouncementDto, long, PagedResultRequestDto, CreateAnnouncementDto,UpdateAnnouncementDto>
{
    Task<List<AnnouncementDto>> GetAllAnnouncement(GetAnnouncementApiInput input);
    Task<DataTableOutputDto<AnnouncementDto>> GetPaged(GetAnnouncementInput input);
}
