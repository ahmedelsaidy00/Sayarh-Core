using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Providers
{
    [Serializable]
    [Table("MainProviders")]
    [DisableAuditing]
    public class MainProvider : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public AccountType AccountType { get; set; }

        public virtual string Code { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }

        public virtual string Avatar { get; set; }

        public virtual string Iban { get; set; }
        public virtual string AccountNumber { get; set; }// رقم الحساب


        public virtual string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public virtual string RegNo { get; set; }
        public virtual string RegNoFilePath { get; set; }
        public virtual string TaxNo { get; set; }
        public virtual string TaxNoFilePath { get; set; }
        public virtual string BankName { get; set; }
        public virtual string AccountName { get; set; }

        public virtual string City { get; set; }
        public virtual string Region { get; set; }
        public virtual string District { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string BoxNumber { get; set; }
        public virtual string Street { get; set; }
        public virtual string BuildingNumber { get; set; }
        public virtual string AdditionalCode { get; set; }
        public virtual string UnitNumber { get; set; }

        public virtual bool IsFuel { get; set; }
        public virtual bool IsClean { get; set; }
        public virtual bool IsMaintain { get; set; }


        public virtual bool AddExternalInvoice { get; set; } // إعداد ارفاق الفاتورة الإلكترونية
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}
