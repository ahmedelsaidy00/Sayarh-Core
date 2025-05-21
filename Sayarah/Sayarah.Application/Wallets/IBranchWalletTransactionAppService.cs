using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Wallets.Dto;

namespace Sayarah.Application.Wallets
{
    public interface IBranchWalletTransactionAppService : IAsyncCrudAppService<BranchWalletTransactionDto, long, GetAllBranchWalletTransactions, CreateBranchWalletTransactionDto, UpdateBranchWalletTransactionDto>
    {
        Task<DataTableOutputDto<BranchWalletTransactionDto>> GetPaged(GetAllBranchWalletTransactionsInput input);
        Task<bool> DeleteTransaction(EntityDto<long> input);
        Task<bool> ManageTransferWallet(ManageTransferWalletInput input);
        Task<BranchWalletTransactionDto> CreateFuelWalletTransaction(CreateBranchWalletTransactionDto input);
        Task<bool> RecalculateBranchAmounts();
        //Task<CheckBranchWalletOutput> CheckBranchWallet(CheckBranchWalletInput input);

        Task CalculateBranchBalanceById(EntityDto<long> input);
    }
}

