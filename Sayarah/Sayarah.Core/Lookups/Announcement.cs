using Abp.Auditing;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace Sayarah.Lookups
{
    [Table("Announcements")]
    [Serializable]
    [DisableAuditing]
    public class Announcement : AuditedEntity<long>
    {
        public virtual string FilePath { get; set; } // for mobile
        public virtual bool IsDefault { get; set; }
        public virtual bool IsVisible { get; set; }
        public virtual AnnouncementType AnnouncementType { get; set; }
        public virtual AnnouncementUserType AnnouncementUserType { get; set; }
        public virtual string Url { get; set; }
    }
}
