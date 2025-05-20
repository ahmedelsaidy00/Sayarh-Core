using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Sayarah.EntityFramework
{
    public class SayarahDbContextFactory : IDesignTimeDbContextFactory<SayarahDbContext>
    {
        public SayarahDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<SayarahDbContext>();

            // Load configuration (you can change path if needed)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json") // Make sure this exists in your startup project
                .Build();

            var connectionString = configuration.GetConnectionString("Default");

            builder.UseSqlServer(connectionString);

            return new SayarahDbContext(builder.Options);
        }
    }
}
