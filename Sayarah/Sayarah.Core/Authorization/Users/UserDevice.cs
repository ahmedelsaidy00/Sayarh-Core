using Abp.Domain.Entities;
using Sayarah.Helpers.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Authorization.Users
{
    [Table("UserDevices")]
    public class UserDevice : Entity<long>
    {
        public virtual long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual string RegistrationToken { get; set; }
        public virtual DeviceType DeviceType { get; set; }
    }
}
