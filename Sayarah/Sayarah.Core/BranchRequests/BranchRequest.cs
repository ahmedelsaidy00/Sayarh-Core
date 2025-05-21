using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.BranchRequests
{
    [Serializable]
    [Table("BranchRequests")]
    [Audited]
    public class BranchRequest : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        [DisableAuditing]
        public virtual string Code { get; set; }
        public virtual string Quantity { get; set; }
        public virtual DeliveyWays? DeliveyWay { get; set; }
        public virtual DateTime? DeliveyDate { get; set; }
        [DisableAuditing]
        public virtual string Notes { get; set; }

        public virtual RequestStatus Status { get; set; }
        [DisableAuditing]
        public virtual FuelType FuelType { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
