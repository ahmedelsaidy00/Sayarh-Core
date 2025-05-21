using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Drivers;
using Sayarah.Interfaces;
using Sayarah.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("Veichles")]
    [Audited]
    public class Veichle : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual string Code { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }

        public virtual string PlateNumber { get; set; }
        public virtual string PlateNumberEn { get; set; }

        public virtual string PlateLetters { get; set; }
        public virtual string PlateLettersEn { get; set; }

        public virtual string FullPlateNumber { get; set; }
        public virtual string FullPlateNumberAr { get; set; }


        public virtual string BodyNumber { get; set; }
        public virtual string SimNumber { get; set; }
        public virtual FuelType? FuelType { get; set; }
        public virtual int? TankSize { get; set; }
        public virtual int? FuelAverageUsage { get; set; }
        public virtual int? KiloCount { get; set; }

        public virtual decimal Fuel_In { get; set; }
        public virtual decimal Fuel_Out { get; set; }
        public virtual decimal Fuel_Balance { get; set; }

        public virtual decimal Maintain_In { get; set; }
        public virtual decimal Maintain_Out { get; set; }
        public virtual decimal Maintain_Balance { get; set; }


        public virtual decimal Oil_In { get; set; }
        public virtual decimal Oil_Out { get; set; }
        public virtual decimal Oil_Balance { get; set; }

        public virtual decimal Wash_In { get; set; }
        public virtual decimal Wash_Out { get; set; }
        public virtual decimal Wash_Balance { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual long? FuelGroupId { get; set; }
        [ForeignKey("FuelGroupId")]
        public virtual FuelGroup FuelGroup { get; set; }

        public virtual decimal MoneyBalance { get; set; }
        public virtual decimal FuelLitreBalance { get; set; }
        public virtual DateTime? MoneyBalanceStartDate { get; set; }
        public virtual DateTime? MoneyBalanceEndDate { get; set; }
        public virtual int PeriodScheduleCount { get; set; }

        //public virtual long? BranchProviderId { get; set; }
        //[ForeignKey("BranchProviderId")]
        //public virtual BranchProvider BranchProvider { get; set; }


        public virtual VeichleType? VeichleType { get; set; }
        public virtual long? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        public virtual long? ModelId { get; set; }
        [ForeignKey("ModelId")]
        public virtual Model Model { get; set; }


        // wait migration 
        public virtual string WorkingDays { get; set; }
        public virtual string YearOfIndustry { get; set; }


        public virtual ConsumptionType ConsumptionType { get; set; }

        // save current trip id 
        public virtual long? VeichleTripId { get; set; }
        //[ForeignKey("VeichleTripId")]
        //public virtual VeichleTrip VeichleTrip { get; set; }

        public virtual ICollection<VeichleTrip> VeichleTrips { get; set; }
        //public virtual ICollection<DriverVeichle> DriverVeichles { get; set; }


        // Save current driver
        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual string InternalNumber { get; set; }  // رقم المركبة الداخلي
        public virtual string InternalNumberFilePath { get; set; }


        public virtual bool ActivateTimeBetweenFuelTransaction { get; set; }
        public virtual int TimeBetweenFuelTransaction { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}