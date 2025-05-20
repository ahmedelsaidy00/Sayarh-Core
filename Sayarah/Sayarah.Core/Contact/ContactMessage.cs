using Sayarah.Authorization.Users;
using Sayarah.Helpers.Enums;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Contact
{

    [Table("ContactMessages")]
    [Serializable]
    public class ContactMessage : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {

        public virtual string Email { get; set; }
        [Required]
        public virtual ContactsType ContactType { get; set; }
        public virtual SubjectType SubjectType { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Subject { get; set; } // رقم الإعلان
        public virtual string Message { get; set; } // desc 
        public virtual string FilePath { get; set; }
        public virtual string Reply { get; set; }
        public virtual bool IsSeen { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
    public enum ContactsType
    {
        NewsLetter = 0,
        Contact = 1,
        Complaint = 2
    }
    public enum TicketTypes
    {
        Companies = 0,
        MainProviders = 1
    }

}