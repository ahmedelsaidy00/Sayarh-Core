using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using Sayarah.Lookups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Companies
{
    [Serializable]
    [Table("Companies")]
    [DisableAuditing]
    public class Company : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual string Code { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }

        public virtual string Avatar { get; set; }
        public virtual decimal WalletAmount { get; set; }

        public virtual long? CompanyTypeId { get; set; }
        [ForeignKey("CompanyTypeId")]
        public virtual CompanyType CompanyType { get; set; }

        public virtual string RegNo { get; set; } // رقم السجل التجاري
        public virtual string RegNoFilePath { get; set; }
        public virtual string TaxNo { get; set; } // الرقم الضريبي
        public virtual string TaxNoFilePath { get; set; }


        public virtual bool IsFuel { get; set; }
        public virtual bool IsClean { get; set; }
        public virtual bool IsMaintain { get; set; }


        public virtual string City { get; set; }
        public virtual string Region { get; set; }
        public virtual string District { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string BoxNumber { get; set; }
        public virtual string Street { get; set; }
        public virtual string BuildingNumber { get; set; }
        public virtual string AdditionalCode { get; set; }
        public virtual string UnitNumber { get; set; }


        public virtual bool CounterPicIsRequired { get; set; } // صورة العداد إلزامية ؟

        public virtual bool ActivateTimeBetweenFuelTransaction { get; set; }
        public virtual int TimeBetweenFuelTransaction { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}
