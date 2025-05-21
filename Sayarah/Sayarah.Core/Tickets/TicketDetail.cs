using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Tickets
{
    [Serializable]
    [Table("TicketDetails")]
    [Audited]
    public class TicketDetail : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        [DisableAuditing]
        public virtual string Code { get; set; }

        public virtual long? TicketId { get; set; }
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }


        public virtual TicketFrom TicketFrom { get; set; }
        public virtual string Description { get; set; }
        [DisableAuditing]
        public virtual string FilePath { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}
