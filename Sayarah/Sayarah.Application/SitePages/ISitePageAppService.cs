using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.SitePages.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sayarah.Application.SitePages
{
    public interface ISitePageAppService : IAsyncCrudAppService<SitePageDto, int, GetAllSitePages, CreateSitePageDto, UpdateSitePageDto>
    {
        Task<bool> Manage(ManageSitePageDto input);
        Task<List<SitePageDto>> ApiGetAll(GetAllSitePages input);
        Task<bool> ChangeSort(UpdateSortInput input);
        Task<SitePageDto> UpdateStatus(EntityDto<int> input);
        Task<SitePageDto> CreateSlider(CreateSitePageDto input);
        //Task<bool> GetPaymentLive();
    }
}
