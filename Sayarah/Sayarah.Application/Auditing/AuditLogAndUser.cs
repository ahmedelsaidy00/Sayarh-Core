using Abp.Auditing;
using Sayarah.Authorization.Users;

namespace Sayarah.Application.auditing;

public class AuditLogAndUser
{
    public AuditLog AuditLog { get; set; }
    public User User { get; set; }
}