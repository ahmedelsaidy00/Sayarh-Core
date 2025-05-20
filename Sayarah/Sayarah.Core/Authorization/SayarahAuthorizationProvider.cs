using Abp.Authorization;
using Abp.Localization;
using static Sayarah.Authorization.PermissionNames;
using static Sayarah.Authorization.PermissionNames.MainProviderData;

namespace Sayarah.Authorization
{
    public class SayarahAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            UsersAndRolesPermissions(context);
            CompanyPermissions(context);
            MainProvidersPermissions(context);
            BranchPermissions(context);
            ProviderPermissions(context);
        }
        void UsersAndRolesPermissions(IPermissionDefinitionContext context)
        {
            var UsersAndRolesPermissions = context.CreatePermission("UsersAndRolesPermissions", L("Permission.UsersAndRolesPermissions"));

            // Settings
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Settings.Read, L("Permission.Settings"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Settings.Write, L("Permission.Settings"));

            // Admins
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Admins.Read, L("Permission.Admins"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Admins.Write, L("Permission.Admins"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Admins.Delete, L("Permission.Admins"));

            // AdminPermissions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminPermissions.Read, L("Permission.AdminPermissions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminPermissions.Write, L("Permission.AdminPermissions"));

            //// Roles
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Roles.Read, L("Permission.Roles"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Roles.Write, L("Permission.Roles"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Roles.Delete, L("Permission.Roles"));

            //// RolePermissions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RolePermissions.Read, L("Permission.RolePermissions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RolePermissions.Write, L("Permission.RolePermissions"));




            // SitePages
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SitePages.Read, L("Permission.SitePages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SitePages.Write, L("Permission.SitePages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SitePages.Delete, L("Permission.SitePages"));

            // Cities
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Cities.Read, L("Permission.Cities"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Cities.Write, L("Permission.Cities"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Cities.Delete, L("Permission.Cities"));

            // Brands
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Brands.Read, L("Permission.Brands"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Brands.Write, L("Permission.Brands"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Brands.Delete, L("Permission.Brands"));

            // Models
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Models.Read, L("Permission.Models"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Models.Write, L("Permission.Models"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Models.Delete, L("Permission.Models"));

            // Packages
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Packages.Read, L("Permission.Packages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Packages.Write, L("Permission.Packages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Packages.Delete, L("Permission.Packages"));

            // Subscriptions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Subscriptions.Read, L("Permission.Subscriptions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Subscriptions.Write, L("Permission.Subscriptions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Subscriptions.Delete, L("Permission.Subscriptions"));

            // ContactMessages
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ContactMessages.Read, L("Permission.ContactMessages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ContactMessages.Write, L("Permission.ContactMessages"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ContactMessages.Delete, L("Permission.ContactMessages"));

            //// SendNotifications
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SendNotifications.Read, L("Permission.SendNotifications"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SendNotifications.Write, L("Permission.SendNotifications"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.SendNotifications.Delete, L("Permission.SendNotifications"));

            //  Notifications
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Notifications.Read, L("Permission.Notifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Notifications.Write, L("Permission.Notifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Notifications.Delete, L("Permission.Notifications"));

            ////Clients
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Clients.Read, L("Permission.Clients"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Clients.Write, L("Permission.Clients"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Clients.Delete, L("Permission.Clients")); 

            //Companies
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Companies.Read, L("Permission.Companies"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Companies.Write, L("Permission.Companies"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Companies.Delete, L("Permission.Companies"));

            // CompanyPermissions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.CompanyPermissions.Read, L("Permission.CompanyPermissions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.CompanyPermissions.Write, L("Permission.CompanyPermissions"));


            //  AdminCompanyWallets
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminCompanyWallets.Read, L("Permission.AdminCompanyWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminCompanyWallets.Write, L("Permission.AdminCompanyWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminCompanyWallets.Delete, L("Permission.AdminCompanyWallets"));

            //Branches
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Branches.Read, L("Permission.Branches"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Branches.Write, L("Permission.Branches"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Branches.Delete, L("Permission.Branches"));

            //Veichles
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Veichles.Read, L("Permission.Veichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Veichles.Write, L("Permission.Veichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Veichles.Delete, L("Permission.Veichles"));

            //Drivers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Drivers.Read, L("Permission.Drivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Drivers.Write, L("Permission.Drivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Drivers.Delete, L("Permission.Drivers"));


            //MainProviders
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.MainProviders.Read, L("Permission.MainProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.MainProviders.Write, L("Permission.MainProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.MainProviders.Delete, L("Permission.MainProviders"));

            // MainProvidersPermissions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.MainProvidersPermissions.Read, L("Permission.MainProvidersPermissions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.MainProvidersPermissions.Write, L("Permission.MainProvidersPermissions"));



            //AdminProviders
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviders.Read, L("Permission.AdminProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviders.Write, L("Permission.AdminProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviders.Delete, L("Permission.AdminProviders"));

            //Workers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Workers.Read, L("Permission.Workers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Workers.Write, L("Permission.Workers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Workers.Delete, L("Permission.Workers"));


            // MainProvidersPermissions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ProvidersPermissions.Read, L("Permission.ProvidersPermissions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ProvidersPermissions.Write, L("Permission.ProvidersPermissions"));



            //BranchRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.BranchRequests.Read, L("Permission.BranchRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.BranchRequests.Write, L("Permission.BranchRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.BranchRequests.Delete, L("Permission.BranchRequests"));

            //RequestOrders
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RequestOrders.Read, L("Permission.RequestOrders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RequestOrders.Write, L("Permission.RequestOrders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RequestOrders.Delete, L("Permission.RequestOrders"));


            //  AdminFuelPriceChangeRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPriceChangeRequests.Read, L("Permission.AdminFuelPriceChangeRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPriceChangeRequests.Write, L("Permission.AdminFuelPriceChangeRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPriceChangeRequests.Delete, L("Permission.AdminFuelPriceChangeRequests"));

            //  AdminFuelTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelTransactions.Read, L("Permission.AdminFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelTransactions.Write, L("Permission.AdminFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelTransactions.Delete, L("Permission.AdminFuelTransactions"));


            //  AdminProviderRevenue
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviderRevenue.Read, L("Permission.AdminProviderRevenue"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviderRevenue.Write, L("Permission.AdminProviderRevenue"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminProviderRevenue.Delete, L("Permission.AdminProviderRevenue"));



            //  AdminInvoices
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminInvoices.Read, L("Permission.AdminInvoices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminInvoices.Write, L("Permission.AdminInvoices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminInvoices.Delete, L("Permission.AdminInvoices"));

            // AdminVouchers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminVouchers.Read, L("Permission.AdminVouchers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminVouchers.Write, L("Permission.AdminVouchers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminVouchers.Delete, L("Permission.AdminVouchers"));

            // AdminJournals
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminJournals.Read, L("Permission.AdminJournals"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminJournals.Write, L("Permission.AdminJournals"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminJournals.Delete, L("Permission.AdminJournals"));

            // Banks
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Banks.Read, L("Permission.Banks"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Banks.Write, L("Permission.Banks"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.Banks.Delete, L("Permission.Banks"));


            // ChipDevices
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipDevices.Read, L("Permission.ChipDevices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipDevices.Write, L("Permission.ChipDevices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipDevices.Delete, L("Permission.ChipDevices"));


            // ChipNumbers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipNumbers.Read, L("Permission.ChipNumbers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipNumbers.Write, L("Permission.ChipNumbers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.ChipNumbers.Delete, L("Permission.ChipNumbers"));


            // RegisterationRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RegisterationRequests.Read, L("Permission.RegisterationRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RegisterationRequests.Write, L("Permission.RegisterationRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.RegisterationRequests.Delete, L("Permission.RegisterationRequests"));


            // CompanyTypes
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.CompanyTypes.Read, L("Permission.CompanyTypes"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.CompanyTypes.Write, L("Permission.CompanyTypes"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.CompanyTypes.Delete, L("Permission.CompanyTypes"));

            // AdminFuelPumps
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPumps.Read, L("Permission.AdminFuelPumps"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPumps.Write, L("Permission.AdminFuelPumps"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminFuelPumps.Delete, L("Permission.AdminFuelPumps"));
            
            // AdminFuelPumps
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AnnouncmentBanners.Read, L("Permission.AnnouncmentBanners"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AnnouncmentBanners.Write, L("Permission.AnnouncmentBanners"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AnnouncmentBanners.Delete, L("Permission.AnnouncmentBanners"));


            // AdminHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminHelpAndSupport.Read, L("Permission.AdminHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminHelpAndSupport.Write, L("Permission.AdminHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminHelpAndSupport.Delete, L("Permission.AdminHelpAndSupport"));

           
            // EntityChanges
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.EntityChanges.Read, L("Permission.EntityChanges"));

            
            // AdminHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminEmployees.Read, L("Permission.AdminEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminEmployees.Write, L("Permission.AdminEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.AdminEmployees.Delete, L("Permission.AdminEmployees"));

            //FrequencyReport
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.AdminsData.FrequencyReport.Read, L("Permission.FrequencyReport"));

        }

        void CompanyPermissions(IPermissionDefinitionContext context)
        {
            var UsersAndRolesPermissions = context.CreatePermission("CompanyPermissions", L("Permission.CompanyPermissions"));


            // CompanyBranchInfo
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchInfo.Read, L("Permission.CompanyBranchInfo"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchInfo.Write, L("Permission.CompanyBranchInfo"));

            //  CompanyBranchEmployees
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchEmployees.Read, L("Permission.CompanyBranchEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchEmployees.Write, L("Permission.CompanyBranchEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchEmployees.Delete, L("Permission.CompanyBranchEmployees"));


            //  CompanyBranchNotifications
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchNotifications.Read, L("Permission.CompanyBranchNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchNotifications.Write, L("Permission.CompanyBranchNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchNotifications.Delete, L("Permission.CompanyBranchNotifications"));

            //  CompanyBranches
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranches.Read, L("Permission.CompanyBranches"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranches.Write, L("Permission.CompanyBranches"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranches.Delete, L("Permission.CompanyBranches"));

 

            //  Veichles
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchVeichles.Read, L("Permission.CompanyBranchVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchVeichles.Write, L("Permission.CompanyBranchVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchVeichles.Delete, L("Permission.CompanyBranchVeichles"));

            //  VeichleTransferRecords
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTransferRecords.Read, L("Permission.VeichleTransferRecords"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTransferRecords.Write, L("Permission.VeichleTransferRecords"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTransferRecords.Delete, L("Permission.VeichleTransferRecords"));

            //  VeichleTripsRecords
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTripsRecords.Read, L("Permission.VeichleTripsRecords"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTripsRecords.Write, L("Permission.VeichleTripsRecords"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleTripsRecords.Delete, L("Permission.VeichleTripsRecords"));


            //  CompanyBranchDrivers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDrivers.Read, L("Permission.CompanyBranchDrivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDrivers.Write, L("Permission.CompanyBranchDrivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDrivers.Delete, L("Permission.CompanyBranchDrivers"));


            //  CompanyBranchDriverVeichles
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDriverVeichles.Read, L("Permission.CompanyBranchDriverVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDriverVeichles.Write, L("Permission.CompanyBranchDriverVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchDriverVeichles.Delete, L("Permission.CompanyBranchDriverVeichles"));


            // CompanySubscriptions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanySubscriptions.Read, L("Permission.CompanySubscriptions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanySubscriptions.Write, L("Permission.CompanySubscriptions"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanySubscriptions.Delete, L("Permission.CompanySubscriptions"));


            //  CompanyWallets
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyWallets.Read, L("Permission.CompanyWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyWallets.Write, L("Permission.CompanyWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyWallets.Delete, L("Permission.CompanyWallets"));

            //  BranchWallets
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchWallets.Read, L("Permission.CompanyBranchWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchWallets.Write, L("Permission.CompanyBranchWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchWallets.Delete, L("Permission.CompanyBranchWallets"));


            //  CompanyBranchFuelTransOuts
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchFuelTransOuts.Read, L("Permission.CompanyBranchFuelTransOuts"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchFuelTransOuts.Write, L("Permission.CompanyBranchFuelTransOuts"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchFuelTransOuts.Delete, L("Permission.CompanyBranchFuelTransOuts"));


            //BranchesLogin
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.BranchesLogin.Read, L("Permission.BranchesLogin"));


 

            //  CompanyFuelTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchFuelTransactions.Read, L("Permission.CompanyBranchFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyBranchFuelTransactions.Write, L("Permission.CompanyBranchFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyBranchFuelTransactions.Delete, L("Permission.CompanyBranchFuelTransactions"));


            ////  CompanyMaintainTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyMaintainTransactions.Read, L("Permission.CompanyMaintainTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyMaintainTransactions.Write, L("Permission.CompanyMaintainTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyMaintainTransactions.Delete, L("Permission.CompanyMaintainTransactions"));


            ////  CompanyWashTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyWashTransactions.Read, L("Permission.CompanyWashTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyWashTransactions.Write, L("Permission.CompanyWashTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyWashTransactions.Delete, L("Permission.CompanyWashTransactions"));


            //CompanyBranchRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchRequests.Read, L("Permission.CompanyBranchRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchRequests.Write, L("Permission.CompanyBranchRequests"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchRequests.Delete, L("Permission.CompanyBranchRequests"));


            //  CompanyBranchProviders
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchProviders.Read, L("Permission.CompanyBranchProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchProviders.Write, L("Permission.CompanyBranchProviders"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchProviders.Delete, L("Permission.CompanyBranchProviders"));

            // MainProviderAccountStatement
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchAccountStatement.Read, L("Permission.CompanyBranchAccountStatement"));

            //CompanyReports
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchReports.Read, L("Permission.CompanyBranchReports"));
            
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchInvoices.Read, L("Permission.CompanyBranchInvoices"));


            //  CompanyBranchHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchHelpAndSupport.Read, L("Permission.CompanyBranchHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchHelpAndSupport.Write, L("Permission.CompanyBranchHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyBranchHelpAndSupport.Delete, L("Permission.CompanyBranchHelpAndSupport"));


            //CompanyFrequencyReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyFrequencyReport.Read, L("Permission.CompanyFrequencyReport"));

            //CompanyConsumptionReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.CompanyConsumptionReport.Read, L("Permission.CompanyConsumptionReport"));

            //VeichleConsumptionReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.CompanyData.VeichleConsumptionReport.Read, L("Permission.VeichleConsumptionReport"));

        }

        void BranchPermissions(IPermissionDefinitionContext context)
        {
            var UsersAndRolesPermissions = context.CreatePermission("BranchPermissions", L("Permission.BranchPermissions"));


            // BranchInfo
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchInfo.Read, L("Permission.BranchInfo"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchInfo.Write, L("Permission.BranchInfo"));

            //  BranchEmployees
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchEmployees.Read, L("Permission.BranchEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchEmployees.Write, L("Permission.BranchEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchEmployees.Delete, L("Permission.BranchEmployees"));

            //  BranchVeichles
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichles.Read, L("Permission.BranchVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichles.Write, L("Permission.BranchVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichles.Delete, L("Permission.BranchVeichles"));

            //  BranchVeichleRoutes
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichleRoutes.Read, L("Permission.BranchVeichleRoutes"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichleRoutes.Write, L("Permission.BranchVeichleRoutes"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichleRoutes.Delete, L("Permission.BranchVeichleRoutes"));

            //  BranchFuelGroups
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelGroups.Read, L("Permission.BranchFuelGroups"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelGroups.Write, L("Permission.BranchFuelGroups"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelGroups.Delete, L("Permission.BranchFuelGroups"));



            //  Drivers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDrivers.Read, L("Permission.BranchDrivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDrivers.Write, L("Permission.BranchDrivers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDrivers.Delete, L("Permission.BranchDrivers"));



            //  BranchDriverVeichles
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDriverVeichles.Read, L("Permission.BranchDriverVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDriverVeichles.Write, L("Permission.BranchDriverVeichles"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchDriverVeichles.Delete, L("Permission.BranchDriverVeichles"));

            //  BranchFuelTransIns
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransIns.Read, L("Permission.BranchFuelTransIns"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransIns.Write, L("Permission.BranchFuelTransIns"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransIns.Delete, L("Permission.BranchFuelTransIns"));

            //  BranchFuelTransOuts
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransOuts.Read, L("Permission.BranchFuelTransOuts"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransOuts.Write, L("Permission.BranchFuelTransOuts"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransOuts.Delete, L("Permission.BranchFuelTransOuts"));


            //  BranchNotifications
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchNotifications.Read, L("Permission.BranchNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchNotifications.Write, L("Permission.BranchNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchNotifications.Delete, L("Permission.BranchNotifications"));


            //  BranchWallets
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchWallets.Read, L("Permission.BranchWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchWallets.Write, L("Permission.BranchWallets"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchWallets.Delete, L("Permission.BranchWallets"));


            //  ProviderHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchHelpAndSupport.Read, L("Permission.BranchHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchHelpAndSupport.Write, L("Permission.BranchHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchHelpAndSupport.Delete, L("Permission.BranchHelpAndSupport"));

            //BranchFrequencyReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFrequencyReport.Read, L("Permission.BranchFrequencyReport"));

            //CompanyConsumptionReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchConsumptionReport.Read, L("Permission.BranchConsumptionReport"));

            //BranchVeichleConsumptionReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchVeichleConsumptionReport.Read, L("Permission.BranchVeichleConsumptionReport"));

            //GasStationLocations
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.GasStationLocations.Read, L("Permission.GasStationLocations"));

            //CarChargingStations
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.CarChargingStations.Read, L("Permission.CarChargingStations"));

            //BranchFuelTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchData.BranchFuelTransactions.Read, L("Permission.BranchFuelTransactions"));

            ////BranchRequests
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchRequests.Read, L("Permission.BranchRequests"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchRequests.Write, L("Permission.BranchRequests"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.BranchRequests.Delete, L("Permission.BranchRequests"));

        }

        void MainProvidersPermissions(IPermissionDefinitionContext context)
        {
            var UsersAndRolesPermissions = context.CreatePermission("MainProvidersPermissions", L("Permission.MainProvidersPermissions"));


            // MainProviderInfo
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderInfo.Read, L("Permission.MainProviderInfo"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderInfo.Write, L("Permission.MainProviderInfo"));

            //  MainProvidersEmployees
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProvidersEmployees.Read, L("Permission.MainProvidersEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProvidersEmployees.Write, L("Permission.MainProvidersEmployees"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProvidersEmployees.Delete, L("Permission.MainProvidersEmployees"));

            //  Providers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.Providers.Read, L("Permission.Providers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.Providers.Write, L("Permission.Providers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.Providers.Delete, L("Permission.Providers"));


            //  MainProviderNotifications
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderNotifications.Read, L("Permission.MainProviderNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderNotifications.Write, L("Permission.MainProviderNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderNotifications.Delete, L("Permission.MainProviderNotifications"));

            //  MainProviderWorkers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWorkers.Read, L("Permission.MainProviderWorkers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWorkers.Write, L("Permission.MainProviderWorkers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWorkers.Delete, L("Permission.MainProviderWorkers"));

            //  WorkerTransferRecords
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.WorkerTransferRecords.Read, L("Permission.WorkerTransferRecords"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.WorkerTransferRecords.Write, L("Permission.WorkerTransferRecords"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.WorkerTransferRecords.Delete, L("Permission.WorkerTransferRecords"));



            //  MainProviderFuelPriceChangeRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPriceChangeRequests.Read, L("Permission.MainProviderFuelPriceChangeRequests"));
            // UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPriceChangeRequests.Write, L("Permission.MainProviderFuelPriceChangeRequests"));
            // UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPriceChangeRequests.Delete, L("Permission.MainProviderFuelPriceChangeRequests"));

            //  MainProviderStationTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderStationTransactions.Read, L("Permission.MainProviderStationTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderStationTransactions.Write, L("Permission.MainProviderStationTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderStationTransactions.Delete, L("Permission.MainProviderStationTransactions"));


            //  MainProviderFuelTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelTransactions.Read, L("Permission.MainProviderFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelTransactions.Write, L("Permission.MainProviderFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelTransactions.Delete, L("Permission.MainProviderFuelTransactions"));


            //  MainProviderMaintainTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderMaintainTransactions.Read, L("Permission.MainProviderMaintainTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderMaintainTransactions.Write, L("Permission.MainProviderMaintainTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderMaintainTransactions.Delete, L("Permission.MainProviderMaintainTransactions"));

            ////  MainProviderOilTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderOilTransactions.Read, L("Permission.MainProviderOilTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderOilTransactions.Write, L("Permission.MainProviderOilTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderOilTransactions.Delete, L("Permission.MainProviderOilTransactions"));


            //  MainProviderWashTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWashTransactions.Read, L("Permission.MainProviderWashTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWashTransactions.Write, L("Permission.MainProviderWashTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderWashTransactions.Delete, L("Permission.MainProviderWashTransactions"));


            //  MainProviderInvoices
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderInvoices.Read, L("Permission.MainProviderInvoices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderInvoices.Write, L("Permission.MainProviderInvoices"));
            // UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderInvoices.Delete, L("Permission.MainProviderInvoices"));


            //  MainProviderVouchers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderVouchers.Read, L("Permission.MainProviderVouchers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderVouchers.Write, L("Permission.MainProviderVouchers"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderVouchers.Delete, L("Permission.MainProviderVouchers"));

            // MainProviderAccountStatement
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderAccountStatement.Read, L("Permission.MainProviderAccountStatement"));


            ////  ProviderReports
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderReports.Read, L("Permission.MainProviderReports"));
            //// UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.ProviderReports.Write, L("Permission.ProviderReports"));
            ////  UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.ProviderReports.Delete, L("Permission.ProviderReports"));


            ////  MainProviderJournals
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderJournals.Read, L("Permission.MainProviderJournals"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.Journals.Write, L("Permission.Journals"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.Journals.Delete, L("Permission.Journals"));


            // StationsLogin
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.StationsLogin.Read, L("Permission.StationsLogin"));


            ////  MainProviderFuelPumps
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPumps.Read, L("Permission.MainProviderFuelPumps"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPumps.Write, L("Permission.MainProviderFuelPumps"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderFuelPumps.Delete, L("Permission.MainProviderFuelPumps"));

         

            //  CompanyHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderHelpAndSupport.Read, L("Permission.MainProviderHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderHelpAndSupport.Write, L("Permission.MainProviderHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderHelpAndSupport.Delete, L("Permission.MainProviderHelpAndSupport"));


            //  MainProviderConsumptionReport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.MainProviderData.MainProviderConsumptionReport.Read, L("Permission.MainProviderConsumptionReport"));
            

        }


        void ProviderPermissions(IPermissionDefinitionContext context)
        {
            var UsersAndRolesPermissions = context.CreatePermission("ProviderPermissions", L("Permission.ProviderPermissions"));

            // ProviderInfo
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderInfo.Read, L("Permission.ProviderInfo"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderInfo.Write, L("Permission.ProviderInfo"));



            //  StationTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.StationTransactions.Read, L("Permission.StationTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.StationTransactions.Write, L("Permission.StationTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.StationTransactions.Delete, L("Permission.StationTransactions"));


            //  ProviderFuelTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelTransactions.Read, L("Permission.ProviderFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelTransactions.Write, L("Permission.ProviderFuelTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelTransactions.Delete, L("Permission.ProviderFuelTransactions"));


            //  MaintainTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderMaintainTransactions.Read, L("Permission.ProviderMaintainTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderMaintainTransactions.Write, L("Permission.ProviderMaintainTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderMaintainTransactions.Delete, L("Permission.ProviderMaintainTransactions"));

            ////  OilTransactions
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.OilTransactions.Read, L("Permission.OilTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.OilTransactions.Write, L("Permission.OilTransactions"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.OilTransactions.Delete, L("Permission.OilTransactions"));


            //  WashTransactions
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWashTransactions.Read, L("Permission.ProviderWashTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWashTransactions.Write, L("Permission.ProviderWashTransactions"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWashTransactions.Delete, L("Permission.ProviderWashTransactions"));



            //  Invoices
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderInvoices.Read, L("Permission.ProviderInvoices"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderInvoices.Write, L("Permission.ProviderInvoices"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderInvoices.Delete, L("Permission.ProviderInvoices"));

            ////  ElectronicInvoices
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ElectronicInvoices.Read, L("Permission.ElectronicInvoices"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ElectronicInvoices.Write, L("Permission.ElectronicInvoices"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ElectronicInvoices.Delete, L("Permission.ElectronicInvoices"));

            //  InvoicesPayments
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderVouchers.Read, L("Permission.ProviderVouchers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderVouchers.Write, L("Permission.ProviderVouchers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderVouchers.Delete, L("Permission.ProviderVouchers"));



            // ProviderAccountStatement
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderAccountStatement.Read, L("Permission.ProviderAccountStatement"));



            //  FuelPriceChangeRequests
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelPriceChangeRequests.Read, L("Permission.ProviderFuelPriceChangeRequests"));
           // UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelPriceChangeRequests.Write, L("Permission.ProviderFuelPriceChangeRequests"));
           // UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderFuelPriceChangeRequests.Delete, L("Permission.ProviderFuelPriceChangeRequests"));



            ////  ProviderReports
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderReports.Read, L("Permission.ProviderReports"));
            //// UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderReports.Write, L("Permission.ProviderReports"));
            ////  UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderReports.Delete, L("Permission.ProviderReports"));


            //  Workers
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWorkers.Read, L("Permission.ProviderWorkers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWorkers.Write, L("Permission.ProviderWorkers"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderWorkers.Delete, L("Permission.ProviderWorkers"));

            ////  Journals
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderJournals.Read, L("Permission.ProviderJournals"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderJournals.Write, L("Permission.ProviderJournals"));
            ////UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderJournals.Delete, L("Permission.ProviderJournals"));

            //  ProviderNotifications
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderNotifications.Read, L("Permission.ProviderNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderNotifications.Write, L("Permission.ProviderNotifications"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderNotifications.Delete, L("Permission.ProviderNotifications"));


            ////  FuelPumps
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.FuelPumps.Read, L("Permission.FuelPumps"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.FuelPumps.Write, L("Permission.FuelPumps"));
            //UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.FuelPumps.Delete, L("Permission.FuelPumps"));



            //  ProviderHelpAndSupport
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderHelpAndSupport.Read, L("Permission.ProviderHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderHelpAndSupport.Write, L("Permission.ProviderHelpAndSupport"));
            UsersAndRolesPermissions.CreateChildPermission(PermissionNames.ProviderData.ProviderHelpAndSupport.Delete, L("Permission.ProviderHelpAndSupport"));





        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SayarahConsts.LocalizationSourceName);
        }
    }
}
