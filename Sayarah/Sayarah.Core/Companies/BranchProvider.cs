using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using Sayarah.Providers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Companies
{
    [Serializable]
    [Table("BranchProviders")]
    [DisableAuditing]
    public class BranchProvider : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }


        public virtual decimal BranchProvider_In { get; set; }
        public virtual decimal BranchProvider_Out { get; set; }
        public virtual decimal BranchProvider_Balance { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
