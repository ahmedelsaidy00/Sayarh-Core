using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using Sayarah.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Tickets
{
    [Serializable]
    [Table("Tickets")]
    [Audited]
    public class Ticket : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        [DisableAuditing]
        public virtual string Code { get; set; }

        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }


        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }



        [DisableAuditing]
        public virtual ProblemCategory ProblemCategory { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketFrom TicketFrom { get; set; }
        public virtual string Description { get; set; }
        [DisableAuditing]
        public virtual string FilePath { get; set; }
        public virtual string Comment { get; set; }
        public virtual decimal Rate { get; set; }
        public virtual bool IsRated { get; set; }


        public virtual ICollection<TicketDetail> TicketDetails { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
