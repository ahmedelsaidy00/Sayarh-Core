using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Sayarah.Authorization.Users;
using Abp.Auditing;
using Sayarah.Core.Helpers;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("VeichlePics")]
    [DisableAuditing]
    public class VeichlePic : AuditedEntity<long, User>
    {
        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual string Code { get; set; }

        public virtual string FilePath { get; set; }
        public virtual string FileTitle { get; set; }
        public virtual FileType? FileType { get; set; }
    }
}
