using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("FuelPriceChangeRequests")]
    [Audited]
    public class FuelPriceChangeRequest : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual string Code { get; set; }
        public virtual FuelType FuelType { get; set; }
        public virtual decimal OldPrice { get; set; }
        public virtual decimal NewPrice { get; set; }
        public virtual string FilePath { get; set; }
        public virtual ChangeRequestStatus Status { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}