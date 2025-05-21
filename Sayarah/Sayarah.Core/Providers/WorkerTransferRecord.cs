using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("WorkerTransferRecords")]
    [Audited]
    public class WorkerTransferRecord : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual string Code { get; set; }

        public virtual long? SourceProviderId { get; set; }
        [ForeignKey("SourceProviderId")]
        public virtual Provider SourceProvider { get; set; }


        public virtual long? TargetProviderId { get; set; }
        [ForeignKey("TargetProviderId")]
        public virtual Provider TargetProvider { get; set; }


        public virtual long? WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        public virtual Worker Worker { get; set; }


        public virtual TransferStatus? Status { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}