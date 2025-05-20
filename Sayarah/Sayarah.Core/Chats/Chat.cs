using Abp.Domain.Entities.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Chats
{
    [Table("Chats")]
    [Serializable]
    public class Chat : CreationAuditedEntity<long>
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual DateTime? LastMessageDate { get; set; }
        public virtual string LastMessage { get; set; }
        public virtual bool IsSeen { get; set; }
        public virtual int UnReadCountAdmin { get; set; }
        public virtual int UnReadCountUser { get; set; }
        public virtual MessageFrom MessageFrom { get; set; } // 0 admin 1 user

    }
}
