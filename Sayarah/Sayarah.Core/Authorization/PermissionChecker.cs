using Abp.Authorization;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;

namespace Sayarah.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {

        }
    }
}
