
using Abp.Auditing;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using Sayarah.Lookups;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Sayarah.Wallets
{
    [Table("CompanyWalletTransactions")]
    [Serializable]
    [Audited]
    public class CompanyWalletTransaction : AuditedEntity<long>
    {
        public virtual decimal Amount { get; set; }
        public virtual TransactionType TransactionType { get; set; }

        public virtual string Code { get; set; }
        public virtual string Note { get; set; }
        //for payment Api
        public virtual string TrackId { get; set; }
        public virtual string TransactionId { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual PayMethod PayMethod { get; set; }

        public virtual decimal FalseAmount { get; set; }
        public virtual string ReceiptImage { get; set; }
        public virtual DepositStatus DepositStatus { get; set; }

        public virtual long? BankId { get; set; }
        [ForeignKey("BankId")]
        public virtual Bank Bank { get; set; }


        //Client
        [Required, Range(1, long.MaxValue)]
        public virtual long CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }


        public virtual long? SubscriptionId { get; set; }
    }
}