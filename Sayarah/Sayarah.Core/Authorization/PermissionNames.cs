using Sayarah.Providers;

namespace Sayarah.Authorization
{
    public static class PermissionNames
    {
        #region UsersAndRolesPermissions

        public static class AdminsData
        {
            public static class Settings
            {
                public const string Read = "Read.Permission.Settings";
                public const string Write = "Write.Permission.Settings";
            }
            public static class Admins
            {
                public const string Read = "Read.Permission.Admins";
                public const string Write = "Write.Permission.Admins";
                public const string Delete = "Delete.Permission.Admins";
            }

            public static class AdminPermissions
            {
                public const string Read = "Read.Permission.AdminPermissions";
                public const string Write = "Write.Permission.AdminPermissions";
            }

            //public static class Roles
            //{
            //    public const string Read = "Read.Permission.Roles";
            //    public const string Write = "Write.Permission.Roles";
            //    public const string Delete = "Delete.Permission.Roles";
            //}
            //public static class RolePermissions
            //{
            //    public const string Read = "Read.Permission.RolePermissions";
            //    public const string Write = "Write.Permission.RolePermissions";
            //}


            public static class SitePages
            {
                public const string Read = "Read.Permission.SitePages";
                public const string Write = "Write.Permission.SitePages";
                public const string Delete = "Delete.Permission.SitePages";
            }

            public static class Cities
            {
                public const string Read = "Read.Permission.Cities";
                public const string Write = "Write.Permission.Cities";
                public const string Delete = "Delete.Permission.Cities";
            }

            public static class Brands
            {
                public const string Read = "Read.Permission.Brands";
                public const string Write = "Write.Permission.Brands";
                public const string Delete = "Delete.Permission.Brands";
            }

            public static class Models
            {
                public const string Read = "Read.Permission.Models";
                public const string Write = "Write.Permission.Models";
                public const string Delete = "Delete.Permission.Models";
            }


            public static class Packages
            {
                public const string Read = "Read.Permission.Packages";
                public const string Write = "Write.Permission.Packages";
                public const string Delete = "Delete.Permission.Packages";
            }
            public static class Subscriptions
            {
                public const string Read = "Read.Permission.Subscriptions";
                public const string Write = "Write.Permission.Subscriptions";
                public const string Delete = "Delete.Permission.Subscriptions";
            }


            public static class ContactMessages
            {
                public const string Read = "Read.Permission.ContactMessages";
                public const string Write = "Write.Permission.ContactMessages";
                public const string Delete = "Delete.Permission.ContactMessages";
            }


            public static class Notifications
            {
                public const string Read = "Read.Permission.Notifications";
                public const string Write = "Write.Permission.Notifications";
                public const string Delete = "Delete.Permission.Notifications";
            }


            public static class Companies
            {
                public const string Read = "Read.Permission.Companies";
                public const string Write = "Write.Permission.Companies";
                public const string Delete = "Delete.Permission.Companies";
            }


            public static class CompanyPermissions
            {
                public const string Read = "Read.Permission.CompanyPermissions";
                public const string Write = "Write.Permission.CompanyPermissions";
            }


            public static class AdminCompanyWallets
            {
                public const string Read = "Read.Permission.AdminCompanyWallets";
                public const string Write = "Write.Permission.AdminCompanyWallets";
                public const string Delete = "Delete.Permission.AdminCompanyWallets";
            }


            public static class Veichles
            {
                public const string Read = "Read.Permission.Veichles";
                public const string Write = "Write.Permission.Veichles";
                public const string Delete = "Delete.Permission.Veichles";
            }

            public static class Drivers
            {
                public const string Read = "Read.Permission.Drivers";
                public const string Write = "Write.Permission.Drivers";
                public const string Delete = "Delete.Permission.Drivers";
            }


            public static class Branches
            {
                public const string Read = "Read.Permission.Branches";
                public const string Write = "Write.Permission.Branches";
                public const string Delete = "Delete.Permission.Branches";
            }



            public static class MainProviders
            {
                public const string Read = "Read.Permission.MainProviders";
                public const string Write = "Write.Permission.MainProviders";
                public const string Delete = "Delete.Permission.MainProviders";
            }

            public static class MainProvidersPermissions
            {
                public const string Read = "Read.Permission.MainProvidersPermissions";
                public const string Write = "Write.Permission.MainProvidersPermissions";
            }


            public static class AdminProviders
            {
                public const string Read = "Read.Permission.AdminProviders";
                public const string Write = "Write.Permission.AdminProviders";
                public const string Delete = "Delete.Permission.AdminProviders";
            }

            public static class  Workers
            {
                public const string Read = "Read.Permission.Workers";
                public const string Write = "Write.Permission.Workers";
                public const string Delete = "Delete.Permission.Workers";
            }


            public static class ProvidersPermissions
            {
                public const string Read = "Read.Permission.ProvidersPermissions";
                public const string Write = "Write.Permission.ProvidersPermissions";
            }


            public static class BranchRequests
            {
                public const string Read = "Read.Permission.BranchRequests";
                public const string Write = "Write.Permission.BranchRequests";
                public const string Delete = "Delete.Permission.BranchRequests";
            }


            public static class RequestOrders
            {
                public const string Read = "Read.Permission.RequestOrders";
                public const string Write = "Write.Permission.RequestOrders";
                public const string Delete = "Delete.Permission.RequestOrders";
            }


            public static class AdminFuelPriceChangeRequests
            {
                public const string Read = "Read.Permission.AdminFuelPriceChangeRequests";
                public const string Write = "Write.Permission.AdminFuelPriceChangeRequests";
                public const string Delete = "Delete.Permission.AdminFuelPriceChangeRequests";
            }


            public static class AdminInvoices
            {
                public const string Read = "Read.Permission.AdminInvoices";
                public const string Write = "Write.Permission.AdminInvoices";
                public const string Delete = "Delete.Permission.AdminInvoices";

            }


            public static class AdminVouchers
            {
                public const string Read = "Read.Permission.AdminVouchers";
                public const string Write = "Write.Permission.AdminVouchers";
                public const string Delete = "Delete.Permission.AdminVouchers";

            }

            public static class AdminJournals
            {
                public const string Read = "Read.Permission.AdminJournals";
                public const string Write = "Write.Permission.AdminJournals";
                public const string Delete = "Delete.Permission.AdminJournals";

            }

            public static class Banks
            {
                public const string Read = "Read.Permission.Banks";
                public const string Write = "Write.Permission.Banks";
                public const string Delete = "Delete.Permission.Banks";
            }


            public static class ChipDevices
            {
                public const string Read = "Read.Permission.ChipDevices";
                public const string Write = "Write.Permission.ChipDevices";
                public const string Delete = "Delete.Permission.ChipDevices";
            }

            public static class ChipNumbers
            {
                public const string Read = "Read.Permission.ChipNumbers";
                public const string Write = "Write.Permission.ChipNumbers";
                public const string Delete = "Delete.Permission.ChipNumbers";
            }

            public static class RegisterationRequests
            {
                public const string Read = "Read.Permission.RegisterationRequests";
                public const string Write = "Write.Permission.RegisterationRequests";
                public const string Delete = "Delete.Permission.RegisterationRequests";
            }


            public static class CompanyTypes
            {
                public const string Read = "Read.Permission.CompanyTypes";
                public const string Write = "Write.Permission.CompanyTypes";
                public const string Delete = "Delete.Permission.CompanyTypes";
            }


            public static class AdminFuelPumps
            {
                public const string Read = "Read.Permission.AdminFuelPumps";
                public const string Write = "Write.Permission.AdminFuelPumps";
                public const string Delete = "Delete.Permission.AdminFuelPumps";
            }
            
            public static class AnnouncmentBanners
            {
                public const string Read = "Read.Permission.AnnouncmentBanners";
                public const string Write = "Write.Permission.AnnouncmentBanners";
                public const string Delete = "Delete.Permission.AnnouncmentBanners";
            }


            public static class AdminHelpAndSupport
            {
                public const string Read = "Read.Permission.AdminHelpAndSupport";
                public const string Write = "Write.Permission.AdminHelpAndSupport";
                public const string Delete = "Delete.Permission.AdminHelpAndSupport";
            }

            public static class AdminFuelTransactions
            {
                public const string Read = "Read.Permission.AdminFuelTransactions";
                public const string Write = "Write.Permission.AdminFuelTransactions";
                public const string Delete = "Delete.Permission.AdminFuelTransactions";
            }
            public static class AdminProviderRevenue
            {
                public const string Read = "Read.Permission.AdminProviderRevenue";
                public const string Write = "Write.Permission.AdminProviderRevenue";
                public const string Delete = "Delete.Permission.AdminProviderRevenue";
            }

            public static class EntityChanges
            {
                public const string Read = "Read.Permission.EntityChanges";
                public const string Write = "Write.Permission.EntityChanges";
                public const string Delete = "Delete.Permission.EntityChanges";
            }

            public static class AdminEmployees
            {
                public const string Read = "Read.Permission.AdminEmployees";
                public const string Write = "Write.Permission.AdminEmployees";
                public const string Delete = "Delete.Permission.AdminEmployees";
            }

            

            //public static class FrequencyReport
            //{
            //    public const string Read = "Read.Permission.FrequencyReport";
            //    public const string Write = "Write.Permission.FrequencyReport";
            //    public const string Delete = "Delete.Permission.FrequencyReport";
            //}

        }

        public static class CompanyData
        {

            public static class CompanyBranchInfo
            {
                public const string Read = "Read.Permission.CompanyBranchInfo";
                public const string Write = "Write.Permission.CompanyBranchInfo";
            }


            public static class CompanyBranchEmployees
            {
                public const string Read = "Read.Permission.CompanyBranchEmployees";
                public const string Write = "Write.Permission.CompanyBranchEmployees";
                public const string Delete = "Delete.Permission.CompanyBranchEmployees";
            }


            public static class CompanyBranchNotifications
            {
                public const string Read = "Read.Permission.CompanyBranchNotifications";
                public const string Write = "Write.Permission.CompanyBranchNotifications";
                public const string Delete = "Delete.Permission.CompanyBranchNotifications";
            }


            public static class CompanyBranches
            {
                public const string Read = "Read.Permission.CompanyBranches";
                public const string Write = "Write.Permission.CompanyBranches";
                public const string Delete = "Delete.Permission.CompanyBranches";
            }

            public static class CompanyBranchVeichles
            {
                public const string Read = "Read.Permission.CompanyBranchVeichles";
                public const string Write = "Write.Permission.CompanyBranchVeichles";
                public const string Delete = "Delete.Permission.CompanyBranchVeichles";
            }


            public static class VeichleTransferRecords
            {
                public const string Read = "Read.Permission.VeichleTransferRecords";
                public const string Write = "Write.Permission.VeichleTransferRecords";
                public const string Delete = "Delete.Permission.VeichleTransferRecords";
            }


            public static class VeichleTripsRecords
            {
                public const string Read = "Read.Permission.VeichleTripsRecords";
                public const string Write = "Write.Permission.VeichleTripsRecords";
                public const string Delete = "Delete.Permission.VeichleTripsRecords";
            }

            public static class CompanyBranchDrivers
            {
                public const string Read = "Read.Permission.CompanyBranchDrivers";
                public const string Write = "Write.Permission.CompanyBranchDrivers";
                public const string Delete = "Delete.Permission.CompanyBranchDrivers";
            }

            public static class CompanyBranchDriverVeichles
            {
                public const string Read = "Read.Permission.CompanyBranchDriverVeichles";
                public const string Write = "Write.Permission.CompanyBranchDriverVeichles";
                public const string Delete = "Delete.Permission.CompanyBranchDriverVeichles";
            }


            public static class CompanySubscriptions
            {
                public const string Read = "Read.Permission.CompanySubscriptions";
                public const string Write = "Write.Permission.CompanySubscriptions";
                public const string Delete = "Delete.Permission.CompanySubscriptions";
            }


            public static class CompanyWallets
            {
                public const string Read = "Read.Permission.CompanyWallets";
                public const string Write = "Write.Permission.CompanyWallets";
                public const string Delete = "Delete.Permission.CompanyWallets";
            }

            public static class CompanyBranchWallets
            {
                public const string Read = "Read.Permission.CompanyBranchWallets";
                public const string Write = "Write.Permission.CompanyBranchWallets";
                public const string Delete = "Delete.Permission.CompanyBranchWallets";
            }


            public static class CompanyBranchFuelTransOuts
            {
                public const string Read = "Read.Permission.CompanyBranchFuelTransOuts";
                public const string Write = "Write.Permission.CompanyBranchFuelTransOuts";
                public const string Delete = "Delete.Permission.CompanyBranchFuelTransOuts";
            }

            public static class CompanyBranchRequests
            {
                public const string Read = "Read.Permission.CompanyBranchRequests";
                public const string Write = "Write.Permission.CompanyBranchRequests";
                public const string Delete = "Delete.Permission.CompanyBranchRequests";
            }


            public static class BranchesLogin
            {
                public const string Read = "Read.Permission.BranchesLogin";
            }


            public static class CompanyBranchFuelTransactions
            {
                public const string Read = "Read.Permission.CompanyBranchFuelTransactions";
                public const string Write = "Write.Permission.CompanyBranchFuelTransactions";
                public const string Delete = "Delete.Permission.CompanyBranchFuelTransactions";
            }


            public static class CompanyBranchProviders
            {
                public const string Read = "Read.Permission.CompanyBranchProviders";
                public const string Write = "Write.Permission.CompanyBranchProviders";
                public const string Delete = "Delete.Permission.CompanyBranchProviders";
            }

            public static class CompanyBranchAccountStatement
            {
                public const string Read = "Read.Permission.CompanyBranchAccountStatement";
                public const string Write = "Write.Permission.CompanyBranchAccountStatement";
                public const string Delete = "Delete.Permission.CompanyBranchAccountStatement";
            }


            public static class CompanyBranchReports
            {
                public const string Read = "Read.Permission.CompanyBranchReports";
            }


            public static class CompanyBranchHelpAndSupport
            {
                public const string Read = "Read.Permission.CompanyBranchHelpAndSupport";
                public const string Write = "Write.Permission.CompanyBranchHelpAndSupport";
                public const string Delete = "Delete.Permission.CompanyBranchHelpAndSupport";
            }


            public static class CompanyBranchInvoices
            {
                public const string Read = "Read.Permission.CompanyBranchInvoices";
                public const string Write = "Write.Permission.CompanyBranchInvoices";
                public const string Delete = "Delete.Permission.CompanyBranchInvoices";
            }

            public static class CompanyFrequencyReport
            {
                public const string Read = "Read.Permission.CompanyFrequencyReport";
                public const string Write = "Write.Permission.CompanyFrequencyReport";
                public const string Delete = "Delete.Permission.CompanyFrequencyReport";
            }

            public static class CompanyConsumptionReport
            {
                public const string Read = "Read.Permission.CompanyConsumptionReport";
            }
            public static class VeichleConsumptionReport
            {
                public const string Read = "Read.Permission.VeichleConsumptionReport";
            }

        }



        public static class BranchData
        {
            public static class BranchInfo
            {
                public const string Read = "Read.Permission.BranchInfo";
                public const string Write = "Write.Permission.BranchInfo";
            }


            public static class BranchEmployees
            {
                public const string Read = "Read.Permission.BranchEmployees";
                public const string Write = "Write.Permission.BranchEmployees";
                public const string Delete = "Delete.Permission.BranchEmployees";
            }


            public static class BranchVeichles
            {
                public const string Read = "Read.Permission.BranchVeichles";
                public const string Write = "Write.Permission.BranchVeichles";
                public const string Delete = "Delete.Permission.BranchVeichles";
            }


            public static class BranchVeichleRoutes
            {
                public const string Read = "Read.Permission.BranchVeichleRoutes";
                public const string Write = "Write.Permission.BranchVeichleRoutes";
                public const string Delete = "Delete.Permission.BranchVeichleRoutes";
            }


            public static class BranchDrivers
            {
                public const string Read = "Read.Permission.BranchDrivers";
                public const string Write = "Write.Permission.BranchDrivers";
                public const string Delete = "Delete.Permission.BranchDrivers";
            }


            public static class BranchFuelGroups
            {
                public const string Read = "Read.Permission.BranchFuelGroups";
                public const string Write = "Write.Permission.BranchFuelGroups";
                public const string Delete = "Delete.Permission.BranchFuelGroups";
            }


            public static class BranchDriverVeichles
            {
                public const string Read = "Read.Permission.BranchDriverVeichles";
                public const string Write = "Write.Permission.BranchDriverVeichles";
                public const string Delete = "Delete.Permission.BranchDriverVeichles";
            }

            public static class BranchFuelTransIns
            {
                public const string Read = "Read.Permission.BranchFuelTransIns";
                public const string Write = "Write.Permission.BranchFuelTransIns";
                public const string Delete = "Delete.Permission.BranchFuelTransIns";
            }

            public static class BranchFuelTransOuts
            {
                public const string Read = "Read.Permission.BranchFuelTransOuts";
                public const string Write = "Write.Permission.BranchFuelTransOuts";
                public const string Delete = "Delete.Permission.BranchFuelTransOuts";
            }


            public static class BranchNotifications
            {
                public const string Read = "Read.Permission.BranchNotifications";
                public const string Write = "Write.Permission.BranchNotifications";
                public const string Delete = "Delete.Permission.BranchNotifications";
            }


            public static class BranchWallets
            {
                public const string Read = "Read.Permission.BranchWallets";
                public const string Write = "Write.Permission.BranchWallets";
                public const string Delete = "Delete.Permission.BranchWallets";
            }


            public static class BranchHelpAndSupport
            {
                public const string Read = "Read.Permission.BranchHelpAndSupport";
                public const string Write = "Write.Permission.BranchHelpAndSupport";
                public const string Delete = "Delete.Permission.BranchHelpAndSupport";
            }

            public static class BranchFrequencyReport
            {
                public const string Read = "Read.Permission.BranchFrequencyReport";
            }

            public static class BranchConsumptionReport
            {
                public const string Read = "Read.Permission.BranchConsumptionReport";
            }
            public static class BranchVeichleConsumptionReport
            {
                public const string Read = "Read.Permission.BranchVeichleConsumptionReport";
            }

            public static class GasStationLocations
            {
                public const string Read = "Read.Permission.GasStationLocations";
            }

            public static class CarChargingStations
            {
                public const string Read = "Read.Permission.CarChargingStations";
            }

            public static class BranchFuelTransactions
            {
                public const string Read = "Read.Permission.BranchFuelTransactions";
            }

        }




        public static class MainProviderData
        {

            public static class MainProviderInfo
            {
                public const string Read = "Read.Permission.MainProviderInfo";
                public const string Write = "Write.Permission.MainProviderInfo";
            }

            public static class MainProvidersEmployees
            {
                public const string Read = "Read.Permission.MainProvidersEmployees";
                public const string Write = "Write.Permission.MainProvidersEmployees";
                public const string Delete = "Delete.Permission.MainProvidersEmployees";
            }
             

            public static class Providers
            {
                public const string Read = "Read.Permission.Providers";
                public const string Write = "Write.Permission.Providers";
                public const string Delete = "Delete.Permission.Providers";
            }


            public static class MainProviderNotifications
            {
                public const string Read = "Read.Permission.MainProviderNotifications";
                public const string Write = "Write.Permission.MainProviderNotifications";
                public const string Delete = "Delete.Permission.MainProviderNotifications";
            }


            public static class MainProviderWorkers
            {
                public const string Read = "Read.Permission.MainProviderWorkers";
                public const string Write = "Write.Permission.MainProviderWorkers";
                public const string Delete = "Delete.Permission.MainProviderWorkers";
            }

            public static class WorkerTransferRecords
            {
                public const string Read = "Read.Permission.WorkerTransferRecords";
                public const string Write = "Write.Permission.WorkerTransferRecords";
                public const string Delete = "Delete.Permission.WorkerTransferRecords";
            }

            public static class MainProviderFuelPriceChangeRequests
            {
                public const string Read = "Read.Permission.MainProviderFuelPriceChangeRequests";
                public const string Write = "Write.Permission.MainProviderFuelPriceChangeRequests";
                public const string Delete = "Delete.Permission.MainProviderFuelPriceChangeRequests";
            }

            public static class MainProviderStationTransactions
            {
                public const string Read = "Read.Permission.MainProviderStationTransactions";
                public const string Write = "Write.Permission.MainProviderStationTransactions";
                public const string Delete = "Delete.Permission.MainProviderStationTransactions";
            }

            public static class MainProviderFuelTransactions
            {
                public const string Read = "Read.Permission.MainProviderFuelTransactions";
                public const string Write = "Write.Permission.MainProviderFuelTransactions";
                public const string Delete = "Delete.Permission.MainProviderFuelTransactions";
            }

            public static class MainProviderMaintainTransactions
            {
                public const string Read = "Read.Permission.MainProviderMaintainTransactions";
                public const string Write = "Write.Permission.MainProviderMaintainTransactions";
                public const string Delete = "Delete.Permission.MainProviderMaintainTransactions";
            }


            public static class MainProviderOilTransactions
            {
                public const string Read = "Read.Permission.MainProviderOilTransactions";
                public const string Write = "Write.Permission.MainProviderOilTransactions";
                public const string Delete = "Delete.Permission.MainProviderOilTransactions";
            }

            public static class MainProviderWashTransactions
            {
                public const string Read = "Read.Permission.MainProviderWashTransactions";
                public const string Write = "Write.Permission.MainProviderWashTransactions";
                public const string Delete = "Delete.Permission.MainProviderWashTransactions";
            }


            public static class MainProviderInvoices
            {
                public const string Read = "Read.Permission.MainProviderInvoices";
                public const string Write = "Write.Permission.MainProviderInvoices";
                public const string Delete = "Delete.Permission.MainProviderInvoices";

            }

            public static class MainProviderVouchers
            {
                public const string Read = "Read.Permission.MainProviderVouchers";
                public const string Write = "Write.Permission.MainProviderVouchers";
                public const string Delete = "Delete.Permission.MainProviderVouchers";

            }

            public static class MainProviderReports
            {
                public const string Read = "Read.Permission.MainProviderReports";
                public const string Write = "Write.Permission.MainProviderReports";
                public const string Delete = "Delete.Permission.MainProviderReports";
            }


            public static class MainProviderJournals
            {
                public const string Read = "Read.Permission.MainProviderJournals";
                public const string Write = "Write.Permission.MainProviderJournals";
                public const string Delete = "Delete.Permission.MainProviderJournals";

            }

            public static class StationsLogin
            {
                public const string Read = "Read.Permission.StationsLogin";
            }

            public static class MainProviderAccountStatement
            {
                public const string Read = "Read.Permission.MainProviderAccountStatement";
                public const string Write = "Write.Permission.MainProviderAccountStatement";
                public const string Delete = "Delete.Permission.MainProviderAccountStatement";
            }

            public static class MainProviderHelpAndSupport
            {
                public const string Read = "Read.Permission.MainProviderHelpAndSupport";
                public const string Write = "Write.Permission.MainProviderHelpAndSupport";
                public const string Delete = "Delete.Permission.MainProviderHelpAndSupport";
            }
            public static class MainProviderConsumptionReport
            {
                public const string Read = "Read.Permission.MainProviderConsumptionReport";
            }
        }
         

        public static class ProviderData
        {


            public static class ProviderInfo
            {
                public const string Read = "Read.Permission.ProviderInfo";
                public const string Write = "Write.Permission.ProviderInfo";
            }
             
            public static class ProviderNotifications
            {
                public const string Read = "Read.Permission.ProviderNotifications";
                public const string Write = "Write.Permission.ProviderNotifications";
                public const string Delete = "Delete.Permission.ProviderNotifications";
            }
             
            public static class ProviderWorkers
            {
                public const string Read = "Read.Permission.ProviderWorkers";
                public const string Write = "Write.Permission.ProviderWorkers";
                public const string Delete = "Delete.Permission.ProviderWorkers";
            }
             
          
            public static class ProviderFuelPriceChangeRequests
            {
                public const string Read = "Read.Permission.ProviderFuelPriceChangeRequests";
                public const string Write = "Write.Permission.ProviderFuelPriceChangeRequests";
                public const string Delete = "Delete.Permission.ProviderFuelPriceChangeRequests";
            }
             
            public static class ProviderInvoices
            {
                public const string Read = "Read.Permission.ProviderInvoices";
                public const string Write = "Write.Permission.ProviderInvoices";
                public const string Delete = "Delete.Permission.ProviderInvoices"; 
            }

             
            public static class ProviderVouchers
            {
                public const string Read = "Read.Permission.ProviderVouchers";
                public const string Write = "Write.Permission.ProviderVouchers";
                public const string Delete = "Delete.Permission.ProviderVouchers";

            }


            public static class ProviderJournals
            {
                public const string Read = "Read.Permission.ProviderJournals";
                public const string Write = "Write.Permission.ProviderJournals";
                public const string Delete = "Delete.Permission.ProviderJournals";

            }
             
            public static class ProviderReports
            {
                public const string Read = "Read.Permission.ProviderReports";
                public const string Write = "Write.Permission.ProviderReports";
                public const string Delete = "Delete.Permission.ProviderReports"; 
            }
             
            public static class ProviderFuelTransactions
            {
                public const string Read = "Read.Permission.ProviderFuelTransactions";
                public const string Write = "Write.Permission.ProviderFuelTransactions";
                public const string Delete = "Delete.Permission.ProviderFuelTransactions";
            }
             
            public static class ProviderMaintainTransactions
            {
                public const string Read = "Read.Permission.ProviderMaintainTransactions";
                public const string Write = "Write.Permission.ProviderMaintainTransactions";
                public const string Delete = "Delete.Permission.ProviderMaintainTransactions";
            }
             
            public static class ProviderWashTransactions
            {
                public const string Read = "Read.Permission.ProviderWashTransactions";
                public const string Write = "Write.Permission.ProviderWashTransactions";
                public const string Delete = "Delete.Permission.ProviderWashTransactions";
            }

              
            public static class ProviderAccountStatement
            {
                public const string Read = "Read.Permission.ProviderAccountStatement";
                public const string Write = "Write.Permission.ProviderAccountStatement";
                public const string Delete = "Delete.Permission.ProviderAccountStatement";
            }


            public static class ProviderHelpAndSupport
            {
                public const string Read = "Read.Permission.ProviderHelpAndSupport";
                public const string Write = "Write.Permission.ProviderHelpAndSupport";
                public const string Delete = "Delete.Permission.ProviderHelpAndSupport";
            }

        }
         

        #endregion
    }
}