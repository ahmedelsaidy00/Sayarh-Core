using Abp.Auditing;
using Sayarah.Journals;
using Sayarah.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Invoices
{
    [Serializable]
    [Table("Invoices")]
    [Audited]
    public class Invoice : FullAuditedEntity<long>
    {
        public virtual string Code { get; set; }
        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual long? JournalId { get; set; }
        [ForeignKey("JournalId")]
        public virtual Journal Journal { get; set; }

        public virtual decimal Amount { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual decimal Taxes { get; set; }
        public virtual decimal Net { get; set; }

        public virtual DateTime? PeriodFrom { get; set; }
        public virtual DateTime? PeriodTo { get; set; }


        public virtual bool ExternalInvoice { get; set; }
        public virtual string ExternalCode { get; set; }
        public virtual string FilePath { get; set; } // ملف الفاتوره 


        public virtual ICollection<InvoiceTransaction> InvoiceTransactions { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }


    }
}
