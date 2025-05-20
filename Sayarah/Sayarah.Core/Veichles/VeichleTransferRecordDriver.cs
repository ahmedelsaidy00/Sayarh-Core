using System.ComponentModel.DataAnnotations.Schema;
using System;
using Abp.Auditing;
using Abp.Domain.Entities;
using Sayarah.Drivers;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("VeichleTransferRecordDrivers")]
    public class VeichleTransferRecordDriver : Entity<long>
    {
        public virtual long? VeichleTransferRecordId { get; set; }
        [ForeignKey("VeichleTransferRecordId")]
        public virtual VeichleTransferRecord VeichleTransferRecord { get; set; }

        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }
    }
}