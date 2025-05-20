using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Helpers.Enums;
using Sayarah.Interfaces;
using Sayarah.Lookups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.RegisterationRequests
{
    [Serializable]
    [Table("RegisterationRequests")]
    [Audited]
    public class RegisterationRequest : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual long? AccountId { get; set; }
        public virtual AccountType AccountType { get; set; }
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual string UserName { get; set; }
        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string PhoneNumberConfirmationCode { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual bool EmailAddressConfirmed { get; set; }

        public virtual string EmailAddress { get; set; }
        public virtual string EmailAddressConfirmationCode { get; set; }

        public virtual string Password { get; set; }
        public virtual HearAboutUs HearAboutUs { get; set; }

        public virtual RegisterationRequestStatus Status { get; set; }

        public virtual string RefuseReason { get; set; }

        public virtual long? CompanyTypeId { get; set; }
        [ForeignKey("CompanyTypeId")]
        public virtual CompanyType CompanyType { get; set; }


        public virtual string RegNo { get; set; } // رقم السجل التجاري
        public virtual string RegNoFilePath { get; set; }
        public virtual string TaxNo { get; set; } // الرقم الضريبي
        public virtual string TaxNoFilePath { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }


    }
}