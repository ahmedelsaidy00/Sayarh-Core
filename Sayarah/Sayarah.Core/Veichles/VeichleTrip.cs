using Abp.Auditing;
using Abp.Domain.Entities.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("VeichleTrips")]
    [Audited]
    public class VeichleTrip : AuditedEntity<long, User>
    {
        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual string Code { get; set; }
        public virtual string TripNumber { get; set; }
        public virtual decimal MaxLitersCount { get; set; } // أقصى استهلاك للرحلة بالليتر
        public virtual decimal ConsumptionBySar { get; set; } // Trip consumption by SAR
        public virtual decimal CurrentConsumption { get; set; } // الاستهلاك الحالي
        public virtual bool IsActive { get; set; }

        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }

        public virtual string Notes { get; set; }
    }
}
