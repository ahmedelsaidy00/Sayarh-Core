using Abp.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Lookups
{
    [Serializable]
    [Table("Brands")]

    [DisableAuditing]
    public class Brand : AuditedEntity<long>
    {
        public virtual string Code { get; set; }
        [Required]
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }


        //public override string ToString()
        //{
        //    // Return a user-friendly display name format
        //    return $"Brand: {NameAr} ({Code})";
        //}

    }
}