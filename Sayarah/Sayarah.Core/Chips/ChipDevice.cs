using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Chips
{
    [Serializable]
    [Table("ChipDevices")]
    [DisableAuditing]
    public class ChipDevice : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual string Code { get; set; }
        [Required]
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Reason { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}