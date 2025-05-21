using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Sayarah.Authorization.Users;
using Sayarah.Veichles;
using Abp.Auditing;
using Sayarah.Core.Helpers;

namespace Sayarah.CompanyInvoices
{
    [Serializable]
    [Table("CompanyInvoiceTransactions")]
    [DisableAuditing]
    public class CompanyInvoiceTransaction : FullAuditedEntity<long>
    {
        public virtual long? CompanyInvoiceId { get; set; }
        [ForeignKey("CompanyInvoiceId")]
        public virtual CompanyInvoice CompanyInvoice { get; set; }


        public virtual long? VeichleId { get; set; }
        [ForeignKey("VeichleId")]
        public virtual Veichle Veichle { get; set; } 
        public virtual long? TransId { get; set; } 
        public virtual TransOutTypes TransType { get; set; } 
        public virtual decimal Price { get; set; }

        public virtual FuelType? TransFuelType { get; set; } 
        public virtual decimal FuelPrice { get; set; } // سعر لتر الوقود
        public virtual decimal Quantity { get; set; } // litre
    }
}
