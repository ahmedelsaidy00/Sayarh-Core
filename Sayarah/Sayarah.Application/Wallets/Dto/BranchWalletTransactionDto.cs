using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using Sayarah.Application.Companies.Dto;
using Sayarah.Wallets;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Wallets.Dto
{
    [AutoMapFrom(typeof(BranchWalletTransaction))]
    public class BranchWalletTransactionDto : AuditedEntityDto<long>
    {
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public string TrackId { get; set; }
        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PayMethod PayMethod { get; set; }
        public decimal FalseAmount { get; set; }


        public long BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }

        public WalletType WalletType { get; set; }
    }
     

    [AutoMapTo(typeof(BranchWalletTransaction))]
    public class CreateBranchWalletTransactionDto
    {
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public string TrackId { get; set; }
        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PayMethod PayMethod { get; set; }
        public decimal FalseAmount { get; set; }
        public long BranchId { get; set; }

        public bool IgnoreCompanyTransaction { get; set; }
        public bool NotifyBranch { get; set; }
        public PayMethod NotifyType { get; set; }

        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public WalletType WalletType { get; set; }
        public long VeichleId { get; set; } 
        public OperationType? OperationType { get; set; }
        public decimal Price { get; set; }
        public decimal Reserved { get; set; } 
        public bool IsTransOperation { get; set; }

    }

    [AutoMapTo(typeof(BranchWalletTransaction))]
    public class UpdateBranchWalletTransactionDto : EntityDto<long>
    {
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public string TrackId { get; set; }
        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public PayMethod PayMethod { get; set; }
        public decimal FalseAmount { get; set; }
        public long BranchId { get; set; }

        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public WalletType WalletType { get; set; }

    }
    public class GetAllBranchWalletTransactions : PagedResultRequestDto
    {
        public TransactionType? TransactionType { get; set; }
        public bool MaxCount { get; set; }
        public long? BranchId { get; set; }
        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public WalletType? WalletType { get; set; }

    }
    public class GetAllBranchWalletTransactionsInput : DataTableInputDto
    {
        public string Code { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public TransactionType? TransactionType { get; set; }
        public long? UserId { get; set; }
        public long? BranchId { get; set; }
        public long? CreatorUserId { get; set; }
        public string Note { get; set; }

        public long? TransId { get; set; }
        public TransOutTypes? TransType { get; set; }
        public WalletType? WalletType { get; set; }

    }




    public class ManageTransferWalletInput
    {
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public long TragetBranchId { get; set; }
        public long BranchId { get; set; }

        public bool NotifyCurrentBranch { get; set; }
        public WalletType WalletType { get; set; }


    }


    public class CheckBranchWalletInput
    {
        public decimal Amount { get; set; }
        public long BranchId { get; set; }
        public WalletType? WalletType { get; set; }

    }

    public class CheckBranchWalletOutput
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

}
