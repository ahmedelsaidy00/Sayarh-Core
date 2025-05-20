using Abp.Domain.Entities.Auditing;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Reports
{
    [Serializable]
    [Table("Reports")]
    public class Report : AuditedEntity<long>
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }
        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual string FilePath { get; set; }
        public virtual string Text { get; set; }
        public virtual string AudioPath { get; set; }
    }
}
