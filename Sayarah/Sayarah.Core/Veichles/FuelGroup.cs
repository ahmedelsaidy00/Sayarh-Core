using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Veichles
{
    [Serializable]
    [Table("FuelGroups")]
    //[DisableAuditing]
    public class FuelGroup : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public virtual string Code { get; set; }

        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }

        public virtual GroupType GroupType { get; set; }
        public virtual PeriodConsumptionType PeriodConsumptionType { get; set; } // لو نوع الاستهلاك بمدة

        public virtual decimal Amount { get; set; }
        public virtual PeriodType? PeriodType { get; set; }
        public virtual bool Transferable { get; set; } // قابل للترحيل

        public virtual decimal LitersCount { get; set; } // عدد اللترات
        public virtual decimal MaximumRechargeAmount { get; set; } //أقصى مبلغ للتعبئة لو نوع الاستهلاك مفتوح

        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}
