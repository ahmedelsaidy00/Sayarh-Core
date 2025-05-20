using Abp.Auditing;
using Abp.Domain.Entities.Auditing;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Invoices
{
    [Serializable]
    [Table("InvoiceDetails")]
    [DisableAuditing]
    public class InvoiceDetail : FullAuditedEntity<long>
    {
        public virtual long? InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        public virtual int Serial { get; set; }
        public virtual long? ItemId { get; set; }
        public virtual AccountTypes AccountType { get; set; }

        public virtual decimal Price { get; set; }
        public virtual bool IsTaxable { get; set; }
        public virtual string Note { get; set; }
    }
}
