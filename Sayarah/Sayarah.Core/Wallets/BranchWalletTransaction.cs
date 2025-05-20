using Abp.Auditing;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Sayarah.Wallets
{
    [Table("BranchWalletTransactions")]
    [Serializable]
    [Audited]
    public class BranchWalletTransaction : AuditedEntity<long>
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


        [Required, Range(1, long.MaxValue)]
        public virtual long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }


        public virtual long? TransId { get; set; }
        public virtual TransOutTypes? TransType { get; set; }


        public virtual WalletType WalletType { get; set; }
    }
}