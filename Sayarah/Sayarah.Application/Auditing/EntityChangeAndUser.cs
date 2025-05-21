using Abp.EntityHistory;
using Sayarah.Authorization.Users;

namespace Sayarah.Application.auditing;
public class EntityChangeAndUser
{
    public EntityChange EntityChange { get; set; }
    public User User { get; set; }
}