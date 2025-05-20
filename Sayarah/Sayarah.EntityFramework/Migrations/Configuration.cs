using Sayarah.EntityFramework;
using Sayarah.Migrations.SeedData;

namespace Sayarah.Migrations
{
    using Abp.MultiTenancy;
    using Microsoft.EntityFrameworkCore;


    namespace Sayarah.Migrations
    {
        public static class SayarahSeedHelper
        {
            public static void SeedHostDb(SayarahDbContext context)
            {
                // Apply any pending migrations
                context.Database.Migrate();

                // Seeding only for host (Tenant == null)
                new InitialHostDbBuilder(context).Create();
                new DefaultTenantCreator(context).Create();
                new TenantRoleAndUserBuilder(context, 1).Create();

                context.SaveChanges();
            }

            public static void SeedTenantDb(SayarahDbContext context, AbpTenantBase tenant)
            {
                // Example: custom logic per tenant
                // You can check tenant.Id and seed accordingly

                // No more DisableAllFilters in EF Core
                context.SaveChanges();
            }
        }
    }

}
