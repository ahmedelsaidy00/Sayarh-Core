using Abp.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Lookups
{
    [Serializable]
    [Table("Banks")]
    [DisableAuditing]
    public class Bank : AuditedEntity<long>
    {
        public virtual string Code { get; set; }
        [Required]
        public virtual string Name { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string Iban { get; set; }
        public virtual string AccountNumber { get; set; }// رقم الحساب
        public virtual string BeneficiaryName { get; set; } // اسم المستفيد
    }
}