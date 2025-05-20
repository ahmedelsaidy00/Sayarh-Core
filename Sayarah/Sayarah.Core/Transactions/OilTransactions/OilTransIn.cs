using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Interfaces;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Transactions
{
    [Serializable]
    [Table("OilTransIns")]
    [Audited]
    public class OilTransIn : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual string Code { get; set; }
        public virtual int Quantity { get; set; } // kilo
        public virtual string Notes { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}
