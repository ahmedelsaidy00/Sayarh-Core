using Abp.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Lookups
{
    [Serializable]
    [Table("Models")]
    [DisableAuditing]
    public class Model : AuditedEntity<long>
    {
        public virtual string Code { get; set; }
        [Required]
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }

        public virtual long? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

    }
}