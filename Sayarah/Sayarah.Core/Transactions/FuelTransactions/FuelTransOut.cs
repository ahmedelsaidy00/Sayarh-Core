using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Drivers;
using Sayarah.Helpers.Enums;
using Sayarah.Interfaces;
using Sayarah.Providers;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Transactions
{
    [Serializable]
    [Table("FuelTransOuts")]
    [Audited]
    public class FuelTransOut : FullAuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {


        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }

        public virtual long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual Driver Driver { get; set; }

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }


        public virtual long? WorkerId { get; set; }
        [ForeignKey("WorkerId")]
        public virtual Worker Worker { get; set; }


        public virtual decimal Quantity { get; set; } // litre
        public virtual string Code { get; set; }

        public virtual decimal Price { get; set; } // السعر الاجمالي

        public virtual string BeforeBoxPic { get; set; }
        public virtual string AfterBoxPic { get; set; }
        public virtual string BeforeCounterPic { get; set; }
        public virtual string AfterCounterPic { get; set; }


        public virtual long? BranchWalletTransactionId { get; set; }
        public virtual bool Completed { get; set; }

        public virtual FuelType? FuelType { get; set; }
        public virtual decimal FuelPrice { get; set; } // سعر لتر الوقود
        public virtual string InvoiceCode { get; set; } //  رقم فاتورة المحطة
        public virtual string CompanyInvoiceCode { get; set; } //  رقم فاتورة الشركة
        public virtual bool InvoiceStatus { get; set; }



        public virtual GroupType? GroupType { get; set; }
        public virtual PeriodConsumptionType? PeriodConsumptionType { get; set; }
        public virtual ConsumptionType ConsumptionType { get; set; }
        public virtual long? VeichleTripId { get; set; }
        public virtual decimal? BranchBalance { get; set; }
        public virtual decimal? Reserved { get; set; }
        public virtual CancelNote? CancelNote { get; set; }
        public virtual string CancelReason { get; set; }


        public virtual User CreatorUser { get; set; }

        public virtual User LastModifierUser { get; set; }

    }
}
