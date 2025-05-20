using Abp.Domain.Entities;
using Sayarah.Companies;
using Sayarah.Helpers.Enums;
using Sayarah.Providers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Authorization.Users
{
    [Table("UserDashboards")]
    public class UserDashboard : Entity<long>
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public virtual long? ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }
    }
}