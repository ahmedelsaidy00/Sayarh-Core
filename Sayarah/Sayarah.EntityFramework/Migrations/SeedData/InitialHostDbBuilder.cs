using Sayarah.EntityFramework;

namespace Sayarah.Migrations.SeedData
{
    public class InitialHostDbBuilder
    {
        private readonly SayarahDbContext _context;

        public InitialHostDbBuilder(SayarahDbContext context)
        {
            _context = context;
        }

        public void Create()
        {

            new DefaultEditionsCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();
        }
    }
}
