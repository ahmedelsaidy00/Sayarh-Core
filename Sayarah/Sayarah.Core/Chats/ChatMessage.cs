using Abp.Domain.Entities.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Chats
{
    [Table("ChatMessages")]
    [Serializable]
    public class ChatMessage : CreationAuditedEntity<long>
    {

        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual string Message { get; set; }
        public virtual bool IsSeen { get; set; } // is seen for admin 
        public virtual bool IsSeenUser { get; set; }  // is seen for user 
        public virtual MessageFrom MessageFrom { get; set; }
        public virtual long? ChatId { get; set; }
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }
        public MessageType MessageType { get; set; }
    }
}
