using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("FuelPumps")]
    public class FuelPump : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual string Code { get; set; }
        public virtual string QrCode { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}