using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Sayarah.Authorization.Users;
using Sayarah.Providers;
using Sayarah.Helpers.Enums;
using Abp.Auditing;

namespace Sayarah.BranchRequests
{
    [Serializable]
    [Table("RequestOrders")]
    [Audited]
    public class RequestOrder : FullAuditedEntity<long>
    {
        [DisableAuditing]
        public virtual long? BranchRequestId { get; set; }
        [ForeignKey("BranchRequestId")]
        public virtual BranchRequest BranchRequest { get; set; }
        [DisableAuditing]
        public virtual string Code { get; set; } 
        public virtual string Quantity { get; set; } 
        public virtual decimal Price { get; set; } 
        public virtual decimal? Discount { get; set; }
        public virtual PayMethod PayMethod { get; set; }
        [DisableAuditing]
        public virtual string Note { get; set; }
        public virtual FuelType FuelType { get; set; }
    }
}
