using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("Workers")]
    [DisableAuditing]
    public class Worker : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual string Code { get; set; }
        public virtual string Name { get; set; }

        public virtual string PhoneNumber { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string Avatar { get; set; }
        public virtual string Notes { get; set; }

        public virtual bool IsFuel { get; set; }
        public virtual bool IsOil { get; set; }
        public virtual bool IsClean { get; set; }
        public virtual bool IsMaintain { get; set; }
        public User CreatorUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public User LastModifierUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

}
