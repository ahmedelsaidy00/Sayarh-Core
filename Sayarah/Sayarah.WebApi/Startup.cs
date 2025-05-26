using Abp.AspNetCore;
using Abp.Dependency;
using Abp.Runtime.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Sayarah.Api;
using System.Text;

namespace Sayarah.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add ABP module
            services.AddAbp<SayarahWebApiModule>();

            // Configure JWT Authentication
            var key = Encoding.ASCII.GetBytes("401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.RequireHttpsMetadata = false; // only for development
                jwtOptions.SaveToken = true;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = "Sayarah",
                    ValidateAudience = true,
                    ValidAudience = "Sayarah",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = AbpClaimTypes.UserName,  // Important for ABP
                    RoleClaimType = AbpClaimTypes.Role      // Important for ABP
                };
            });

            // Add Controllers with Newtonsoft.Json (ABP Classic requires this)
            services.AddControllers()
                .AddApplicationPart(typeof(SayarahWebApiModule).Assembly)
                .AddControllersAsServices()
                .AddNewtonsoftJson();

            // Add Swagger (optional)
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAbp(); // ABP initialization

            app.UseRouting();

            // Enable authentication & authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sayarah API V1");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller=Home}/{action=Index}/{id?}");
            });
            app.UseEndpoints(endpoints =>
            {             
                //endpoints.MapControllers();

                // Convention-based routing
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/services/app/{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
