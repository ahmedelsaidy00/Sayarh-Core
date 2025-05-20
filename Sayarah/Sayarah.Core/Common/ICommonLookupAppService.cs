using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.AbpZeroTemplate.Common.Dto;
//using Sayarah.AbpZeroTemplate.Editions.Dto;

namespace Sayarah.AbpZeroTemplate.Common
{
    public interface ICommonLookupAppService : IApplicationService
    {
        //Task<ListResultDto<SubscribableEditionComboboxItemDto>> GetEditionsForCombobox(bool onlyFreeItems = false);

        Task<PagedResultDto<NameValueDto>> FindUsers(FindUsersInput input);

        GetDefaultEditionNameOutput GetDefaultEditionName();
    }
}