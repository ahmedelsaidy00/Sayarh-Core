using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Drivers
{
    [Serializable]
    [Table("DriverVeichles")]
    [Audited]
    public class DriverVeichle : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }



        public virtual string Serial { get; set; } // معمول ليه يا ترى ؟؟
        public virtual bool IsCurrent { get; set; }

        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
