using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using Sayarah.Packages;
using System.ComponentModel.DataAnnotations.Schema;
using static Sayarah.SayarahConsts;
using Sayarah.Lookups;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Application.Packages.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Core.Helpers;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Subscriptions.Dto
{
    [AutoMapFrom(typeof(SubscriptionTransaction)), AutoMapTo(typeof(SubscriptionTransaction))]
    public class SubscriptionTransactionDto : AuditedEntityDto<long>
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public  string Code { get; set; }
        public  string QrCode { get; set; }
        public decimal Price { get; set; }
        public DateTime? EndDate { get; set; }
    
        public long? PackageId  { get; set; }
        public PackageDto Package { get; set; }
        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string Desc { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsExpired { get; set; }
        public bool Free { get; set; }
        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public string FullReceiptImage
        {
            get
            {
                if (!string.IsNullOrEmpty(ReceiptImage) && Utilities.CheckExistImage(15, "600x600_" + ReceiptImage))
                    return FilesPath.Wallet.ServerImagePath + "600x600_" + ReceiptImage;
                else
                    return FilesPath.Wallet.DefaultImagePath;
            }
        }
       
        public DepositStatus Status { get; set; }

        public long? CompanyWalletTransactionId { get; set; }
        public long? BankId { get; set; }
        public BankDto Bank { get; set; }
         
        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }

        public int VeichlesCount { get; set; } // عدد السيارات الحالية

        public bool AttachNfc { get; set; }
        public PackageType? PackageType { get; set; }
        public bool AutoRenewal { get; set; }

        public decimal AttachNfcPrice { get; set; } // nfc attach price for each veichle
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public int NfcCount { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }

    }

    [AutoMapTo(typeof(SubscriptionTransaction))]
    public class CreateSubscriptionTransactionDto
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? PackageId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsExpired { get; set; }
        public bool Free { get; set; }
        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public DepositStatus Status { get; set; }
        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }
        public int VeichlesCount { get; set; } // عدد السيارات الحالية
        public bool AttachNfc { get; set; }
        public bool AutoRenewal { get; set; }
        public PackageType? PackageType { get; set; }
        public decimal AttachNfcPrice { get; set; } // nfc attach price for each veichle
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }

        public long? CompanyWalletTransactionId { get; set; }
        public long? BankId { get; set; }

        public int NfcCount { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }

    }
    [AutoMapTo(typeof(SubscriptionTransaction))]
    public class UpdateSubscriptionTransactionDto : EntityDto<long>
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public string Code { get; set; }
        public long? CompanyId { get; set; }
        public long? PackageId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsExpired { get; set; }
        public bool Free { get; set; }
        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public DepositStatus Status { get; set; }
        public int VeichlesFrom { get; set; }
        public int VeichlesTo { get; set; }
        public int VeichlesCount { get; set; } // عدد السيارات الحالية
        public bool AttachNfc { get; set; }
        public bool AutoRenewal { get; set; }
        public PackageType? PackageType { get; set; }
        public decimal AttachNfcPrice { get; set; } // nfc attach price for each veichle
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public long? CompanyWalletTransactionId { get; set; }
        public long? BankId { get; set; }


        public int NfcCount { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }

    }
    public class GetSubscriptionTransactionsInput : DataTableInputDto
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public string Code { get; set; }
        public decimal? Price { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public long? Id { get; set; }
        public long? PackageId { get; set; }
        public string PackageName { get; set; }
        public string UserName { get; set; }
        public long? IsSpecialId { get; set; }
        public bool? Free { get; set; }

        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public DepositStatus? Status { get; set; }
        public PackageType? PackageType { get; set; }

    }
    public class GetAllSubscriptionTransactions : PagedResultRequestDto
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public DateTime? EndDate { get; set; }
        public long? Id { get; set; }
        public long? PackageId { get; set; }
        public long? CompanyId { get; set; }
        public long? UserId { get; set; }
        public string Lang { get; set; }
        public bool MaxCount { get; set; }
        public long? CreatorUserId { get; set; }
        public bool Free { get; set; }

        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public DepositStatus? Status { get; set; }

    }
    [AutoMapFrom(typeof(SubscriptionTransaction))]
    public class SubscriptionTransactionSmallDto : EntityDto<long>
    {
        public SubscriptionTransactionType? TransactionType { get; set; }
        public long? SubscriptionId { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public DateTime? EndDate { get; set; }
        public long? PackageId { get; set; }
        public long? CompanyId { get; set; }
        public bool IsSpecial { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool IsPaid { get; set; }
        public bool Free { get; set; }

        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
    }
    public class GetSubscriptionTransactionsOutput
    {
        public SubscriptionTransactionSmallDto SubscriptionTransaction { get; set; }
        public UserDto User { get; set; }
    }
    
    public class SubscriptionTransactionDataOutput : EntityDto<long>
    {

        public long? PackageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? EndDate { get; set; }
        public long? UserId { get; set; }
        public UserDto User { get; set; }
    }
    
    public class RenewSubscriptionTransactionDto
    {
        public long SubscriptionTransactionId { get; set; }
        public long CompanyId { get; set; }
    }

}
