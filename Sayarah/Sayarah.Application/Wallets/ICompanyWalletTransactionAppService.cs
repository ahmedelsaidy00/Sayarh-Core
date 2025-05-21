using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Wallets.Dto;

namespace Sayarah.Application.Wallets
{
    public interface ICompanyWalletTransactionAppService : IAsyncCrudAppService<CompanyWalletTransactionDto, long, GetAllCompanyWalletTransactions, CreateCompanyWalletTransactionDto, UpdateCompanyWalletTransactionDto>
    {
        Task<DataTableOutputDto<CompanyWalletTransactionDto>> GetPaged(GetAllCompanyWalletTransactionsInput input);
        Task<bool> DeleteTransaction(EntityDto<long> input);
        Task<CompanyWalletTransactionDto> SendTransactionRequest(CreateCompanyWalletTransactionDto input);

        Task<CompanyWalletTransactionDto> RefuseCompanyWalletTransaction(UpdateCompanyWalletTransactionDto input);

        Task<CompanyWalletTransactionDto> GetTransaction(GetAllCompanyWalletTransactionsInput input);

        Task<bool> RecalculateCompanyAmounts();
        Task<CompanyWalletTransactionDto> CreateFromAdmin(CreateFromAdminDto input);
        Task<CompanyWalletTransactionDto> GetFullTransactionData(GetAllCompanyWalletTransactionsInput input);
    }
}

