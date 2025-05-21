using Abp.Auditing;
using Abp.Domain.Entities.Auditing;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Journals
{
    [Serializable]
    [Table("Journals")]
    [DisableAuditing]
    public class Journal : FullAuditedEntity<long>
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }


        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }


        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        public virtual string Code { get; set; }
        public virtual string Notes { get; set; }

        public virtual JournalType JournalType { get; set; }

        public virtual ICollection<JournalDetail> JournalDetails { get; set; }


    }
}
