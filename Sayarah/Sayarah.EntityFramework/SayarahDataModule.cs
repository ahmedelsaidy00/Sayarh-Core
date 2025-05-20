using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sayarah.EntityFramework;
using System.Reflection;

namespace Sayarah
{
    [DependsOn(
        typeof(AbpEntityFrameworkCoreModule), // Use AbpEntityFrameworkCoreModule instead of AbpZeroEntityFrameworkModule if using ABP 4.x+ or abp.io
        typeof(SayarahCoreModule)
    )]
    public class SayarahDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            // Register the DbContext and set connection string here
            Configuration.Modules.AbpEfCore().AddDbContext<SayarahDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    options.DbContextOptions.UseSqlServer(options.ExistingConnection);
                }
                else
                {
                    options.DbContextOptions.UseSqlServer(Configuration.DefaultNameOrConnectionString);
                }
            });

            // Set your connection string name here or in appsettings.json
            Configuration.DefaultNameOrConnectionString = "Default";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
