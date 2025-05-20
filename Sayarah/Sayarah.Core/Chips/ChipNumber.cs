using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using Sayarah.Interfaces;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Chips
{
    [Serializable]
    [Table("ChipNumbers")]
    [Audited]
    public class ChipNumber : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        [DisableAuditing]
        public virtual string Code { get; set; }

        public virtual long? ChipDeviceId { get; set; }
        [ForeignKey("ChipDeviceId")]
        public virtual ChipDevice ChipDevice { get; set; }

        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }


        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }


        public virtual string ReleaseNumber { get; set; }
        public virtual ChipStatus Status { get; set; }

        public virtual DateTime? ActivationDate { get; set; }

        public virtual long? ActivationUserId { get; set; }
        [ForeignKey("ActivationUserId")]
        public virtual User ActivationUser { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}