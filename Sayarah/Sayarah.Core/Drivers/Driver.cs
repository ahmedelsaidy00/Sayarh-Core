using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Drivers
{
    [Serializable]
    [Table("Drivers")]
    [DisableAuditing]

    public class Driver : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Avatar { get; set; }
        public virtual string Licence { get; set; }
        public virtual DateTime? LicenceExpireDate { get; set; }

        public virtual string PhoneNumber { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string Notes { get; set; }
        public string DriverCode { get; set; }

        public virtual ICollection<DriverVeichle> DriverVeichles { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }


    }
}
