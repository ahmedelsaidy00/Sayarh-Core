using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("Providers")]
    [DisableAuditing]
    public class Provider : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }

        public virtual string Code { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }

        public virtual double? Latitude { get; set; }
        public virtual double? Longitude { get; set; }
        public virtual string AddressOnMap { get; set; }

        public virtual string PhoneNumber { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string Avatar { get; set; }
        public virtual string FuelTypes { get; set; }

        public virtual bool IsFuel { get; set; }
        public virtual bool IsOil { get; set; }
        public virtual bool IsClean { get; set; }
        public virtual bool IsMaintain { get; set; }


        public virtual decimal FuelNinetyOnePrice { get; set; }
        public virtual decimal FuelNinetyFivePrice { get; set; }
        public virtual decimal SolarPrice { get; set; }


        public virtual decimal InternalWashingPrice { get; set; }
        public virtual decimal ExternalWashingPrice { get; set; }

        public virtual decimal ThreeThousandKiloOilPrice { get; set; }
        public virtual decimal FiveThousandKiloOilPrice { get; set; }
        public virtual decimal TenThousandKiloOilPrice { get; set; }



        public virtual string Iban { get; set; }
        public virtual string AccountNumber { get; set; }// رقم الحساب
        public virtual string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public virtual string BankName { get; set; }
        public virtual string AccountName { get; set; }


        public virtual int WorkersCount { get; set; } // عدد العمال
        public virtual string ManagerName { get; set; }


        public virtual string City { get; set; }
        public virtual string Region { get; set; }
        public virtual string District { get; set; }
        public virtual string Street { get; set; }
        public virtual string Services { get; set; }

        public virtual bool AddExternalInvoice { get; set; } // إعداد ارفاق الفاتورة الإلكترونية

        public virtual bool VisibleInMap { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }

    }
}
