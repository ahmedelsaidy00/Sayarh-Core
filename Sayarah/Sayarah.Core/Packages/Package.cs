using Abp.Auditing;
using Sayarah.Authorization.Users;
using Sayarah.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Packages
{
    [Table("Packages")]
    [Audited]
    public class Package : AuditedEntity<long>, IHasCreatorAndModeifierUserNavigation
    {
        public virtual string NameAr { get; set; }
        public virtual string NameEn { get; set; }
        public virtual string DescAr { get; set; }
        public virtual string DescEn { get; set; }

        public virtual int VeichlesFrom { get; set; }
        public virtual int VeichlesTo { get; set; }

        public virtual decimal AttachNfcPrice { get; set; }
        public virtual decimal MonthlyPrice { get; set; }
        public virtual decimal YearlyPrice { get; set; }

        public virtual decimal SimPrice { get; set; }

        //public virtual PackageType? PackageType { get; set; }
        //public virtual int Duration { get; set; }
        public virtual bool Free { get; set; }
        public virtual bool Visible { get; set; }

        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
    }
}