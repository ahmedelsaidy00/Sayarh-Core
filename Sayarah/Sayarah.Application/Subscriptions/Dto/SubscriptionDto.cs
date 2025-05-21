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
    [AutoMapFrom(typeof(Subscription)), AutoMapTo(typeof(Subscription))]
    public class SubscriptionDto : AuditedEntityDto<long>
    {
        public  string Code { get; set; }
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

    [AutoMapTo(typeof(Subscription))]
    public class CreateSubscriptionDto
    {
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
    [AutoMapTo(typeof(Subscription))]
    public class UpdateSubscriptionDto : EntityDto<long>
    {
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
    public class GetSubscriptionsInput : DataTableInputDto
    {
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
    public class GetAllSubscriptions : PagedResultRequestDto
    {
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
    [AutoMapFrom(typeof(Subscription))]
    public class SubscriptionSmallDto : EntityDto<long>
    {
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
    public class GetSubscriptionsOutput
    {
        public SubscriptionSmallDto Subscription { get; set; }
        public UserDto User { get; set; }
    }
    
    public class SubscriptionDataOutput : EntityDto<long>
    {

        public long? PackageId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? EndDate { get; set; }
        public long? UserId { get; set; }
        public UserDto User { get; set; }
    }
    
    public class ManagePackageStateInPut
    {
        public long? CompanyId { get; set; }
    }
    
    public class InPackageOutPut
    {
        public bool InPackage { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class SendNotifyInput
    {
        public long PackageId { get; set; }
        public long SubscriptionId { get; set; }
        public long SenderUserId { get; set; }
        public long RecieverUserId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public bool ToAdmins { get; set; }
    }


   
    public class RenewSubscriptionDto
    {
        public long SubscriptionId { get; set; }
        public long CompanyId { get; set; }


    }



    [AutoMapTo(typeof(Subscription))]
    public class UpgradeSubscriptionDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? PackageId { get; set; }
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
        public bool IsExpired { get; set; }
        public PayMethod? PayMethod { get; set; }
        public string ReceiptImage { get; set; }
        public int VeichlesCount { get; set; } // عدد السيارات الحالية
        public bool AttachNfc { get; set; }
        public bool AutoRenewal { get; set; }
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


    
    public class HandleSubscriptionOutput : EntityDto<long>
    {
        public long? SubscriptionTransactionId { get; set; }
        public bool Success { get; set; }
    }



    public class CalculateSupscriptionPriceInput
    {
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public bool AttachNfc { get; set; }
        public PackageType PackageType { get; set; }
        public decimal AttachNfcPrice { get; set; }
        public int VeichlesCount { get; set; }
        public int NfcCount { get; set; }
        public int Tax { get; set; }

    }


    public class CalculateSupscriptionPriceOutput
    {
        public decimal VeichlesCountPrices { get; set; }
        public decimal NfcPrices { get; set; }
        public decimal NetPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Price { get; set; }
    }
}
