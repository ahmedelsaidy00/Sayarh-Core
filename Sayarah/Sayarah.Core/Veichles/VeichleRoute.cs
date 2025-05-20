using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("VeichleRoutes")]
    [Audited]
    public class VeichleRoute : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }


        public virtual string Code { get; set; }

        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }


        public virtual string Notes { get; set; }
        public User CreatorUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public User LastModifierUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
