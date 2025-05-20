using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Interfaces;
using Sayarah.Providers;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Transactions
{
    [Serializable]
    [Table("MaintainTransOuts")]
    [Audited]
    public class MaintainTransOut : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }
        public virtual long? WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        public virtual Worker Worker { get; set; }

        public virtual int Quantity { get; set; } // litre
        public virtual string Code { get; set; }

        public virtual decimal Price { get; set; }
        public virtual string CounterPic { get; set; }

        public virtual long? BranchWalletTransactionId { get; set; }

        public virtual bool Completed { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
