using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using System.Collections.Generic;
using Abp.Auditing;
using Sayarah.Providers;

namespace Sayarah.CompanyInvoices
{
    [Serializable]
    [Table("CompanyInvoices")]
    [Audited]
    public class CompanyInvoice : FullAuditedEntity<long>
    {
        public virtual string Code { get; set; }
        public virtual long? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public virtual long? MainProviderId { get; set; }
        [ForeignKey("MainProviderId")]
        public virtual MainProvider MainProvider { get; set; }

        public virtual decimal Amount { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual decimal Taxes { get; set; }
        public virtual decimal Net { get; set; }
        public virtual decimal VatValue { get; set; }
        public virtual decimal Quantity { get; set; }
        public virtual decimal AmountWithOutVat { get; set; }

        public virtual string Month { get; set; }
        public virtual string Year { get; set; }

        public virtual DateTime? PeriodFrom { get; set; }
        public virtual DateTime? PeriodTo { get; set; }

        public virtual ICollection<CompanyInvoiceTransaction> CompanyInvoiceTransactions { get; set; }

    }
}
