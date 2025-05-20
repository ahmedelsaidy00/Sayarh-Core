using Abp.DynamicEntityProperties;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.BranchRequests;
using Sayarah.Chats;
using Sayarah.Chips;
using Sayarah.Companies;
using Sayarah.CompanyInvoices;
using Sayarah.Contact;
using Sayarah.Drivers;
using Sayarah.Invoices;
using Sayarah.Journals;
using Sayarah.Lookups;
using Sayarah.MultiTenancy;
using Sayarah.Packages;
using Sayarah.Providers;
using Sayarah.RegisterationRequests;
using Sayarah.Reports;
using Sayarah.SitePages;
using Sayarah.Tickets;
using Sayarah.Transactions;
using Sayarah.Veichles;
using Sayarah.Wallets;

namespace Sayarah.EntityFramework
{
    public class SayarahDbContext : AbpZeroDbContext<Tenant, Role, User, SayarahDbContext>
    {
        public SayarahDbContext(DbContextOptions<SayarahDbContext> options) : base(options)
        {

        }

        //TODO: Define an DbSet for your Entities...

        public virtual DbSet<SitePage> SitePages { get; set; }
        public virtual DbSet<ContactMessage> ContactMessages { get; set; }
        public virtual DbSet<UserDevice> UserDevices { get; set; }


        public virtual DbSet<Branch> Branches { get; set; }
        public virtual DbSet<BranchProvider> BranchProviders { get; set; }
        public virtual DbSet<Company> Companies { get; set; }

        public virtual DbSet<Driver> Drivers { get; set; }
        public virtual DbSet<DriverVeichle> DriverVeichles { get; set; }

        public virtual DbSet<MainProvider> MainProviders { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<Worker> Workers { get; set; }
        public virtual DbSet<FuelPriceChangeRequest> FuelPriceChangeRequests { get; set; }


        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Veichle> Veichles { get; set; }
        public virtual DbSet<VeichlePic> VeichlePics { get; set; }
        public virtual DbSet<VeichleRoute> VeichleRoutes { get; set; }
        public virtual DbSet<VeichleTrip> VeichleTrips { get; set; }
        public virtual DbSet<FuelGroup> FuelGroups { get; set; }
        public virtual DbSet<VeichleTransferRecord> VeichleTransferRecords { get; set; }
        public virtual DbSet<VeichleTransferRecordDriver> VeichleTransferRecordDrivers { get; set; }

        public virtual DbSet<Package> Packages { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionTransaction> SubscriptionTransactions { get; set; }

        public virtual DbSet<BranchRequest> BranchRequests { get; set; }
        public virtual DbSet<RequestOrder> RequestOrders { get; set; }

        public virtual DbSet<FuelTransIn> FuelTransIns { get; set; }
        public virtual DbSet<FuelTransOut> FuelTransOuts { get; set; }

        public virtual DbSet<MaintainTransIn> MaintainTransIns { get; set; }
        public virtual DbSet<MaintainTransOut> MaintainTransOuts { get; set; }

        public virtual DbSet<OilTransIn> OilTransIns { get; set; }
        public virtual DbSet<OilTransOut> OilTransOuts { get; set; }

        public virtual DbSet<WashTransIn> WashTransIns { get; set; }
        public virtual DbSet<WashTransOut> WashTransOuts { get; set; }

        public virtual DbSet<Journal> Journals { get; set; }
        public virtual DbSet<JournalDetail> JournalDetails { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<InvoiceTransaction> InvoiceTransactions { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }


        public virtual DbSet<CompanyInvoice> CompanyInvoices { get; set; }
        public virtual DbSet<CompanyInvoiceTransaction> CompanyInvoiceTransactions { get; set; }



        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<CompanyWalletTransaction> CompanyWalletTransactions { get; set; }
        public virtual DbSet<BranchWalletTransaction> BranchWalletTransactions { get; set; }


        public virtual DbSet<ChipDevice> ChipDevices { get; set; }
        public virtual DbSet<ChipNumber> ChipNumbers { get; set; }

        public virtual DbSet<RegisterationRequest> RegisterationRequests { get; set; }

        public virtual DbSet<UserDashboard> UserDashboards { get; set; }
        public virtual DbSet<WorkerTransferRecord> WorkerTransferRecords { get; set; }
        public virtual DbSet<FuelPump> FuelPumps { get; set; }


        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Model> Models { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<CompanyType> CompanyTypes { get; set; }
        public virtual DbSet<Announcement> Announcements { get; set; }

        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketDetail> TicketDetails { get; set; }


        /* NOTE: 
         *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
         *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
         *   pass connection string name to base classes. ABP works either way.
         */


        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> Branch (optional many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Branch)
                .WithMany()               // assuming Branch does not have a collection of Users
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // choose restrict or cascade depending on your needs

            // User -> Company (optional many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany() // assuming Company does not have Users collection
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Provider (optional many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Provider)
                .WithMany()
                .HasForeignKey(u => u.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Worker (optional many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Worker)
                .WithMany()
                .HasForeignKey(u => u.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> MainProvider (optional many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.MainProvider)
                .WithMany()
                .HasForeignKey(u => u.MainProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserDevices relationship (one-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserDevices)
                .WithOne()  // If UserDevice has navigation property back to User, specify it here: .WithOne(d => d.User)
                .HasForeignKey("UserId")  // Assuming FK property name in UserDevice
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserDashboards relationship (one-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserDashboards)
                .WithOne()  // Same note as above
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);


            // MainProvider relationships
            modelBuilder.Entity<MainProvider>()
                .HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MainProvider>()
                .HasOne(mp => mp.CreatorUser)
                .WithMany()
                .HasForeignKey("CreatorUserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MainProvider>()
                .HasOne(mp => mp.LastModifierUser)
                .WithMany()
                .HasForeignKey("LastModifierUserId")
                .OnDelete(DeleteBehavior.Restrict);

            // Provider relationships
            modelBuilder.Entity<Provider>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.MainProvider)
                .WithMany() // If you want, you can define collection navigation in MainProvider
                .HasForeignKey(p => p.MainProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.CreatorUser)
                .WithMany()
                .HasForeignKey("CreatorUserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.LastModifierUser)
                .WithMany()
                .HasForeignKey("LastModifierUserId")
                .OnDelete(DeleteBehavior.Restrict);

            // Worker relationships
            modelBuilder.Entity<Worker>(entity =>
            {
                entity.HasOne(w => w.User)
                    .WithMany()
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.Provider)
                    .WithMany() // You can add ICollection<Worker> Workers in Provider to make this WithMany(p => p.Workers)
                    .HasForeignKey(w => w.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Assuming these are shadow properties (since not in your class explicitly)
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("CreatorUserId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("LastModifierUserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });



            // Branch relationships
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Company)
                .WithMany() // or WithMany(c => c.Branches) if you add collection
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.City)
                .WithMany()
                .HasForeignKey(b => b.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.CreatorUser)
                .WithMany()
                .HasForeignKey("CreatorUserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.LastModifierUser)
                .WithMany()
                .HasForeignKey("LastModifierUserId")
                .OnDelete(DeleteBehavior.Restrict);

            // Company relationships
            modelBuilder.Entity<Company>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasOne(c => c.CompanyType)
                .WithMany()
                .HasForeignKey(c => c.CompanyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasOne(c => c.CreatorUser)
                .WithMany()
                .HasForeignKey("CreatorUserId")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasOne(c => c.LastModifierUser)
                .WithMany()
                .HasForeignKey("LastModifierUserId")
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<DynamicProperty>().Property(p => p.PropertyName).HasMaxLength(250);
            modelBuilder.Entity<DynamicEntityProperty>().Property(p => p.EntityFullName).HasMaxLength(250);

            //modelBuilder.Entity<RequestOrder>().Property(x => x.Price).HasPrecision(18, 6);
            //modelBuilder.Entity<RequestOrder>().Property(x => x.Discount).HasPrecision(18, 6);

            modelBuilder.Entity<BranchProvider>().Property(x => x.BranchProvider_Balance).HasPrecision(18, 6);
            modelBuilder.Entity<BranchProvider>().Property(x => x.BranchProvider_In).HasPrecision(18, 6);
            modelBuilder.Entity<BranchProvider>().Property(x => x.BranchProvider_Out).HasPrecision(18, 6);

            modelBuilder.Entity<Invoice>().Property(x => x.Amount).HasPrecision(18, 6);
            modelBuilder.Entity<Invoice>().Property(x => x.Net).HasPrecision(18, 6);
            modelBuilder.Entity<InvoiceDetail>().Property(x => x.Price).HasPrecision(18, 6);

            modelBuilder.Entity<Voucher>().Property(x => x.Amount).HasPrecision(18, 6);

            modelBuilder.Entity<CompanyInvoice>().Property(x => x.Amount).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyInvoice>().Property(x => x.Net).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyInvoice>().Property(x => x.AmountWithOutVat).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyInvoice>().Property(x => x.VatValue).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyInvoiceTransaction>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyInvoiceTransaction>().Property(x => x.FuelPrice).HasPrecision(18, 6);


            modelBuilder.Entity<JournalDetail>().Property(x => x.Credit).HasPrecision(18, 6);
            modelBuilder.Entity<JournalDetail>().Property(x => x.Debit).HasPrecision(18, 6);

            //modelBuilder.Entity<Package>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<Subscription>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<Subscription>().Property(x => x.NetPrice).HasPrecision(18, 6);
            modelBuilder.Entity<Subscription>().Property(x => x.TaxAmount).HasPrecision(18, 6);

            modelBuilder.Entity<SubscriptionTransaction>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<SubscriptionTransaction>().Property(x => x.NetPrice).HasPrecision(18, 6);
            modelBuilder.Entity<SubscriptionTransaction>().Property(x => x.TaxAmount).HasPrecision(18, 6);

            modelBuilder.Entity<FuelTransOut>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<FuelTransOut>().Property(x => x.FuelPrice).HasPrecision(18, 6);

            modelBuilder.Entity<MaintainTransOut>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<OilTransOut>().Property(x => x.Price).HasPrecision(18, 6);
            modelBuilder.Entity<WashTransOut>().Property(x => x.Price).HasPrecision(18, 6);

            //modelBuilder.Entity<FuelGroup>().Property(x => x.Amount).HasPrecision(18, 6);
            //modelBuilder.Entity<FuelGroup>().Property(x => x.LitersCount).HasPrecision(18, 6);

            modelBuilder.Entity<Veichle>().Property(x => x.Fuel_Balance).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Fuel_In).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Fuel_Out).HasPrecision(18, 6);

            modelBuilder.Entity<Veichle>().Property(x => x.Maintain_Balance).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Maintain_In).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Maintain_Out).HasPrecision(18, 6);

            modelBuilder.Entity<Veichle>().Property(x => x.Oil_Balance).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Oil_In).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Oil_Out).HasPrecision(18, 6);

            modelBuilder.Entity<Veichle>().Property(x => x.Wash_Balance).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Wash_In).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.Wash_Out).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.MoneyBalance).HasPrecision(18, 6);
            modelBuilder.Entity<Veichle>().Property(x => x.FuelLitreBalance).HasPrecision(18, 6);

            modelBuilder.Entity<VeichleTrip>().Property(x => x.ConsumptionBySar).HasPrecision(18, 6);
            modelBuilder.Entity<VeichleTrip>().Property(x => x.CurrentConsumption).HasPrecision(18, 6);
            //modelBuilder.Entity<VeichleTrip>().Property(x => x.MaxLitersCount).HasPrecision(18, 6);


            modelBuilder.Entity<CompanyWalletTransaction>().Property(x => x.Amount).HasPrecision(18, 6);
            modelBuilder.Entity<CompanyWalletTransaction>().Property(x => x.FalseAmount).HasPrecision(18, 6);

            modelBuilder.Entity<BranchWalletTransaction>().Property(x => x.Amount).HasPrecision(18, 6);
            modelBuilder.Entity<BranchWalletTransaction>().Property(x => x.FalseAmount).HasPrecision(18, 6);

            modelBuilder.Entity<Company>().Property(x => x.WalletAmount).HasPrecision(18, 6);

            modelBuilder.Entity<Branch>().Property(x => x.WalletAmount).HasPrecision(18, 6);
            modelBuilder.Entity<Branch>().Property(x => x.FuelAmount).HasPrecision(18, 6);
            modelBuilder.Entity<Branch>().Property(x => x.MaintainAmount).HasPrecision(18, 6);
            modelBuilder.Entity<Branch>().Property(x => x.CleanAmount).HasPrecision(18, 6);
            modelBuilder.Entity<Branch>().Property(x => x.ConsumptionAmount).HasPrecision(18, 6);

        }
    }
}
