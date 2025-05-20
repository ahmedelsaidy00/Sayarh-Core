using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using Sayarah.Lookups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Companies
{
    [Serializable]
    [Table("Branches")]
    //[DisableAuditing]
    public class Branch : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual string Code { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }

        public virtual double? Latitude { get; set; }
        public virtual double? Longitude { get; set; }
        public virtual string AddressOnMap { get; set; }
        public virtual int VeichlesCount { get; set; }

        public virtual long? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }
        public virtual string Region { get; set; }
        public virtual string District { get; set; }

        public virtual decimal WalletAmount { get; set; }
        public virtual decimal ConsumptionAmount { get; set; }

        public virtual decimal FuelAmount { get; set; }
        public virtual decimal CleanAmount { get; set; }
        public virtual decimal MaintainAmount { get; set; }

        public virtual bool ActivateTimeBetweenFuelTransaction { get; set; }
        public virtual int TimeBetweenFuelTransaction { get; set; }
        public virtual decimal Reserved { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
