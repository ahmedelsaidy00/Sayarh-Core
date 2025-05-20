using System.Linq;
using Sayarah.EntityFramework;
using Sayarah.MultiTenancy;

namespace Sayarah.Migrations.SeedData
{
    public class DefaultTenantCreator
    {
        private readonly SayarahDbContext _context;

        public DefaultTenantCreator(SayarahDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateUserAndRoles();
        }

        private void CreateUserAndRoles()
        {
            //Default tenant

            var defaultTenant = _context.Tenants.FirstOrDefault(t => t.TenancyName == Tenant.DefaultTenantName);
            if (defaultTenant == null)
            {
                _context.Tenants.Add(new Tenant {TenancyName = Tenant.DefaultTenantName, Name = Tenant.DefaultTenantName});
                _context.SaveChanges();
            }
        }
    }
}
