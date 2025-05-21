using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Subscriptions.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Subscriptions
{
    public interface ISubscriptionAppService : IAsyncCrudAppService<SubscriptionDto, long, GetAllSubscriptions, CreateSubscriptionDto, UpdateSubscriptionDto>
    {
        Task<DataTableOutputDto<SubscriptionDto>> GetPaged(GetSubscriptionsInput input);
        Task<SubscriptionDto> GetCurrentSubscription();
        Task<InPackageOutPut> UserHasPackage(ManagePackageStateInPut input);
        Task<SubscriptionDto> UpdatePayment(UpdateSubscriptionDto input);
        Task<bool> HandleAfterSubscriptionEndDate(EntityDto<long> input);

        Task<SubscriptionDto> GetSubscription(GetAllSubscriptions input);

        Task<PagedResultDto<SubscriptionDto>> GetCompanySubscriptions(GetAllSubscriptions input);
        Task<HandleSubscriptionOutput> SubscribePackage(CreateSubscriptionDto input);
        Task<HandleSubscriptionOutput> RenewSubscription(RenewSubscriptionDto input);
        Task<HandleSubscriptionOutput> UpgradeSubscription(UpgradeSubscriptionDto input);
    }
}
