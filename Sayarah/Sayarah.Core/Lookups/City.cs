using Abp.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Lookups
{
    [Serializable]
    [Table("Cities")]
    [DisableAuditing]
    public class City : AuditedEntity<long>
    {
        public virtual string Code { get; set; }
        [Required]
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
    }
}