//using Abp.Domain.Entities.Auditing;
//using System.ComponentModel.DataAnnotations.Schema;
//using Sayarah.Authorization.Users;
//using System;
//using System.ComponentModel.DataAnnotations;
//using Sayarah.Companies;
//using Sayarah.Veichles;
//using Sayarah.Helpers.Enums;
//using Abp.Auditing;
//using static Sayarah.SayarahConsts.FilesPath;

//namespace Sayarah.ReplacementChipsRequests
//{
//    [Serializable]
//    [Table("ReplacementChipsRequests")]
//    [Audited]
//    public class ReplacementChipsRequest : FullAuditedEntity<long>
//    {
//        [DisableAuditing]
//        public virtual string Code { get; set; }

//        public virtual long? VeichleId { get; set; }
//        [ForeignKey("VeichleId")]
//        public virtual Veichle Veichle { get; set; }

//        public virtual long? CompanyId { get; set; }
//        [ForeignKey("CompanyId")]
//        public virtual Company Company { get; set; }

//        public virtual long? BranchId { get; set; }
//        [ForeignKey("BranchId")]
//        public virtual Branch Branch { get; set; }

//        public virtual RequestStatus Status { get; set; }


//        public virtual decimal Price { get; set; }

//        public virtual DateTime? ActivationDate { get; set; }
//    }
//}