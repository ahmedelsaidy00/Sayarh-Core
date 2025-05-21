using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using Sayarah.Lookups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Packages
{
    [Table("SubscriptionTransactions")]
    [Audited]
    public class SubscriptionTransaction : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual SubscriptionTransactionType? TransactionType { get; set; }


        public virtual string Code { get; set; }
        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual long? PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }

        public virtual long? SubscriptionId { get; set; }
        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; }

        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }

        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }

        public virtual int VeichlesFrom { get; set; }
        public virtual int VeichlesTo { get; set; }

        public virtual int VeichlesCount { get; set; } // عدد السيارات الحالية
        public virtual bool AttachNfc { get; set; }
        public virtual int NfcCount { get; set; }
        public virtual PackageType? PackageType { get; set; }

        public virtual PayMethod? PayMethod { get; set; }
        public virtual decimal AttachNfcPrice { get; set; } // nfc attach price for each veichle
        public virtual decimal MonthlyPrice { get; set; }
        public virtual decimal YearlyPrice { get; set; }
        public virtual decimal NetPrice { get; set; }
        public virtual decimal Tax { get; set; }
        public virtual decimal TaxAmount { get; set; }
        public virtual decimal Price { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual bool IsPaid { get; set; }
        public virtual bool AutoRenewal { get; set; }

        public virtual long? CompanyWalletTransactionId { get; set; }

        public virtual string ReceiptImage { get; set; }
        public virtual long? BankId { get; set; }
        [ForeignKey("BankId")]
        public virtual Bank Bank { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}