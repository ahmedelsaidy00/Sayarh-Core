using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Subscriptions.Dto;
using System.Threading.Tasks;

namespace Sayarah.Application.Subscriptions
{
    public interface ISubscriptionTransactionAppService : IAsyncCrudAppService<SubscriptionTransactionDto, long, GetAllSubscriptionTransactions, CreateSubscriptionTransactionDto, UpdateSubscriptionTransactionDto>
    {
        Task<DataTableOutputDto<SubscriptionTransactionDto>> GetPaged(GetSubscriptionTransactionsInput input);

        Task<PagedResultDto<SubscriptionTransactionDto>> GetCompanySubscriptionTransactions(GetAllSubscriptionTransactions input);
        Task<SubscriptionTransactionDto> GetSubscriptionTransaction(GetAllSubscriptionTransactions input);
        Task<SubscriptionTransactionDto> PrintSubscriptionDetails(GetAllSubscriptionTransactions input);
    }
}
