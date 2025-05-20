using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using Sayarah.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("VeichleTransferRecords")]
    [Audited]
    public class VeichleTransferRecord : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual string Code { get; set; }

        public virtual long? SourceBranchId { get; set; }
        [ForeignKey("SourceBranchId")]
        public virtual Branch SourceBranch { get; set; }


        public virtual long? TargetBranchId { get; set; }
        [ForeignKey("TargetBranchId")]
        public virtual Branch TargetBranch { get; set; }


        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }


        public virtual TransferStatus? Status { get; set; }


        public virtual bool TransferDrivers { get; set; }

        public virtual ICollection<VeichleTransferRecordDriver> VeichleTransferRecordDrivers { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}