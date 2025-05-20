using Abp.Auditing;
using Sayarah.Helpers.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Journals
{
    [Serializable]
    [Table("JournalDetails")]
    [DisableAuditing]
    public class JournalDetail : FullAuditedEntity<long>
    {
        public virtual long? JournalId { get; set; }
        [ForeignKey("JournalId")]
        public virtual Journal Journal { get; set; }


        public virtual long? AccountId { get; set; }
        public virtual AccountTypes AccountType { get; set; }

        public virtual decimal Debit { get; set; }
        public virtual decimal Credit { get; set; }
        public virtual string Note { get; set; }
    }
}
