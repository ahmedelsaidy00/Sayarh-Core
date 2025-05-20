using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Sayarah.Authorization;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.EntityFramework;
using System.Linq;
using static Sayarah.SayarahConsts.FilesPath;

namespace Sayarah.Migrations.SeedData
{
    public class HostRoleAndUserCreator
    {
        private readonly SayarahDbContext _context;

        public HostRoleAndUserCreator(SayarahDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateHostRoleAndUsers();
        }

        private async void CreateHostRoleAndUsers()
        {
            //Admin role for host

            var adminRoleForHost = _context.Roles.FirstOrDefault(r => r.TenantId == null && r.Name == StaticRoleNames.Host.Admin);
            if (adminRoleForHost == null)
            {

                adminRoleForHost = new Role
                {
                    Name = StaticRoleNames.Host.Admin,
                    DisplayName = StaticRoleNames.Host.Admin,
                    IsStatic = true
                };

                adminRoleForHost.SetNormalizedName();

                _context.Roles.Add(adminRoleForHost);
                _context.SaveChanges();

                //Grant all tenant permissions
                var permissions = PermissionFinder
                    .GetAllPermissions(new SayarahAuthorizationProvider())
                    .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Host))
                    .ToList();

                foreach (var permission in permissions)
                {
                    _context.Permissions.Add(
                        new RolePermissionSetting
                        {
                            Name = permission.Name,
                            IsGranted = true,
                            RoleId = adminRoleForHost.Id
                        });
                }

                _context.SaveChanges();
            }

            //Admin user for tenancy host

            var adminUserForHost = _context.Users.FirstOrDefault(u => u.TenantId == null && u.UserName == User.AdminUserName);
            if (adminUserForHost == null)
            {
                var hasher = new PasswordHasher<User>();

                var adminUser = new User
                {
                    UserName = User.AdminUserName,
                    Name = "System",
                    Surname = "Administrator",
                    EmailAddress = "admin@aspnetboilerplate.com",
                    IsEmailConfirmed = true
                };
                adminUser.Password = hasher.HashPassword(adminUser, User.DefaultPassword);


                // Add the user to the DbSet and save changes
                _context.Users.Add(adminUser);

                adminUserForHost.SetNormalizedNames();

                _context.SaveChanges();

                _context.UserRoles.Add(new UserRole(null, adminUserForHost.Id, adminRoleForHost.Id));
                _context.SaveChanges();
            }
        }
    }
}