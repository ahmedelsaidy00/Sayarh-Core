using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Sayarah.Lookups;
using static Sayarah.SayarahConsts;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Wallets;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Wallets.Dto
{
    [AutoMapFrom(typeof(CompanyWalletTransaction))]
    public class CompanyWalletTransactionDto : AuditedEntityDto<long>
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
        public string ReceiptImage { get; set; }
        public string FullReceiptImage
        {
            get
            {
                if (!string.IsNullOrEmpty(ReceiptImage) && Utilities.CheckExistImage(15, /*"600x600_" +*/ ReceiptImage))
                    return FilesPath.Wallet.ServerImagePath + /*"600x600_" +*/ ReceiptImage;
                else
                    return "";
            }
        }
        public DepositStatus DepositStatus { get; set; }
        public long? BankId { get; set; }
        public BankDto Bank { get; set; }

        public long CompanyId { get; set; }
        public CompanyDto Company { get; set; }

        public long? SubscriptionId { get; set; }
    }
     

    [AutoMapTo(typeof(CompanyWalletTransaction))]
    public class CreateCompanyWalletTransactionDto
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
        public string ReceiptImage { get; set; }
        public DepositStatus DepositStatus { get; set; }
        public long? BankId { get; set; }


        public long CompanyId { get; set; }
        public long? SubscriptionId { get; set; }
    }

    [AutoMapTo(typeof(CompanyWalletTransaction))]
    public class UpdateCompanyWalletTransactionDto : EntityDto<long>
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
        public string ReceiptImage { get; set; }
        public DepositStatus DepositStatus { get; set; }
        public long? BankId { get; set; }
        public long CompanyId { get; set; }
        public long? SubscriptionId { get; set; }
    }
    public class GetAllCompanyWalletTransactions : PagedResultRequestDto
    {
        public TransactionType? TransactionType { get; set; }
        public bool MaxCount { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public DepositStatus? DepositStatus { get; set; }
        public long? BankId { get; set; }

        public long? SubscriptionId { get; set; }
    }
    public class GetAllCompanyWalletTransactionsInput : DataTableInputDto
    {
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public decimal? Amount { get; set; }
        public TransactionType? TransactionType { get; set; }
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public long? CompanyId { get; set; }
        public long? CreatorUserId { get; set; }
        public string Note { get; set; }

        public DepositStatus? DepositStatus { get; set; }
        public long? BankId { get; set; }
        public string Code { get; set; }
        public long? SubscriptionId { get; set; }

    }
    [AutoMapTo(typeof(CompanyWalletTransaction))]
    public class CreateFromAdminDto
    {
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public decimal FalseAmount { get; set; }
        public long CompanyId { get; set; }
        public string ReceiptImage { get; set; }
    }


}
