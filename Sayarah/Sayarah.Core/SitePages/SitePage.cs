using Abp.Auditing;
using Abp.Domain.Entities;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.SitePages
{

    [Table("SitePages")]
    [Serializable]
    [DisableAuditing]
    public class SitePage : Entity
    {
        public virtual string Key { get; set; }
        public virtual string Section { get; set; }
        public virtual string Value { get; set; }
        public virtual PageEnum PageEnum { get; set; }
        public virtual LanguageEnum? Language { get; set; }
        public virtual int? Sort { get; set; }
        public virtual bool? IsHidden { get; set; }
        public virtual long? LessonId { get; set; }
    }

}
