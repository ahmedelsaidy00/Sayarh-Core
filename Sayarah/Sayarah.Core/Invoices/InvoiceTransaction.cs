using Abp.Auditing;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Invoices
{
    [Serializable]
    [Table("InvoiceTransactions")]
    [DisableAuditing]
    public class InvoiceTransaction : FullAuditedEntity<long>
    {
        public virtual long? InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }


        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; }


        public virtual long? TransId { get; set; }
        public virtual TransOutTypes TransType { get; set; }
        public virtual decimal Price { get; set; }

        public virtual FuelType? TransFuelType { get; set; }
        public virtual decimal FuelPrice { get; set; } // سعر لتر الوقود
        public virtual decimal Quantity { get; set; } // litre

        public virtual int Serial { get; set; }
    }
}
