using System.Reflection;
using Abp.Application.Services;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Configuration.Startup;
using Abp.Modules;
using Sayarah.Application;

namespace Sayarah.Api
{
    [DependsOn(typeof(AbpAspNetCoreModule), typeof(SayarahApplicationModule))]
    public class SayarahWebApiModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(SayarahApplicationModule).Assembly,
                    moduleName: "app"
                );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }

        public override void PostInitialize()
        {
            // No anti-forgery or authentication filter setup here; configure in Startup.cs
        }
    }
}
