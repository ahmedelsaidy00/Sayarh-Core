using Abp.Auditing;
using Sayarah.Journals;
using Sayarah.Providers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Invoices
{
    [Serializable]
    [Table("Vouchers")]
    [Audited]
    public class Voucher : FullAuditedEntity<long>
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
        public virtual string Note { get; set; }
        public virtual string FilePath { get; set; }
    }
}
