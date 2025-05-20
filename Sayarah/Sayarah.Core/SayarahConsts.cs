namespace Sayarah
{
    public class SayarahConsts
    {
        public const string LocalizationSourceName = "Sayarah";

        public const bool MultiTenancyEnabled = false;
        //public const string DomainUrl = "http://localhost:6234/";
        public const string DomainUrl = "https://sydashboard.syarahapp.sa/";
        
        public const string DataProtectionProviderName = "SayarahApp";

        public const string LogoPath = "";

        public const string MailLogoPath = "/files/logo/logo-header.png";

        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public const string DefaultPassPhrase = "c88b9181421049579090a25f4dd34daf";


        public const int DefaultPageSize = 10;

        /// <summary>
        /// Maximum allowed page size for paged requests.
        /// </summary>
        public const int MaxPageSize = 1000;



        public static class RolesNames
        {
            public const string Admin = "Admin";
            public const string Client = "Client";
            public const string Company = "Company";
            public const string Employee = "Employee";
            public const string Branch = "Branch";
            public const string Driver = "Driver";
            public const string Provider = "Provider";
            public const string Worker = "Worker";
            public const string MainProvider = "MainProvider";
            public const string AdminEmployee = "AdminEmployee";
        }


        public static class FilesPath
        {
            public static class Users
            {
                public const string Image = "Files\\Users\\";
                public const string ServerImagePath = "Files/Users/";
                public const string DefaultImagePath = "Files/nophoto/400x400-user.png";
            }

            public static class Companies
            {
                public const string Image = "Files\\Companies\\";
                public const string ServerImagePath = "Files/Companies/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class Drivers
            {
                public const string Image = "Files\\Drivers\\";
                public const string ServerImagePath = "Files/Drivers/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class Banks
            {
                public const string Image = "Files\\Banks\\";
                public const string ServerImagePath = "Files/Banks/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class Wallet
            {
                public const string Image = "Files\\Wallet\\";
                public const string ServerImagePath = "Files/Wallet/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class Vouchers
            {
                public const string Image = "Files\\Vouchers\\";
                public const string ServerImagePath = "Files/Vouchers/";
                public const string DefaultImagePath = "Files/nophoto/1080x1080.png";
            }

            public static class Invoices
            {
                public const string Image = "Files\\Invoices\\";
                public const string ServerImagePath = "Files/Invoices/";
                public const string DefaultImagePath = "Files/nophoto/1080x1080.png";
            }


            public static class Blogs
            {
                public const string Image = "Files\\Blogs\\";
                public const string ServerImagePath = "Files/Blogs/";
                public const string DefaultImagePath = "Files/nophoto/800x400.png";
            }
            public static class Veichles
            {
                public const string Image = "Files\\Veichles\\";
                public const string ServerImagePath = "Files/Veichles/";
                public const string DefaultImagePath = "Files/nophoto/800x600.png";
            }
            public static class Providers
            {
                public const string Image = "Files\\Providers\\";
                public const string ServerImagePath = "Files/Providers/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }
            public static class Workers
            {
                public const string Image = "Files\\Workers\\";
                public const string ServerImagePath = "Files/Workers/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }
            public static class FuelTransOut
            {
                public const string Image = "Files\\FuelTransOut\\";
                public const string ServerImagePath = "Files/FuelTransOut/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class MaintainTransOut
            {
                public const string Image = "Files\\MaintainTransOut\\";
                public const string ServerImagePath = "Files/MaintainTransOut/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class OilTransOut
            {
                public const string Image = "Files\\OilTransOut\\";
                public const string ServerImagePath = "Files/OilTransOut/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class WashTransOut
            {
                public const string Image = "Files\\WashTransOut\\";
                public const string ServerImagePath = "Files/WashTransOut/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }

            public static class FuelPriceChangeRequests
            {
                public const string Image = "Files\\FuelPriceChangeRequests\\";
                public const string ServerImagePath = "Files/FuelPriceChangeRequests/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }
            public static class RegisterationRequests
            {
                public const string Image = "Files\\RegisterationRequests\\";
                public const string ServerImagePath = "Files/RegisterationRequests/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }


            public static class Tickets
            {
                public const string Image = "Files\\Tickets\\";
                public const string ServerImagePath = "Files/Tickets/";
                public const string DefaultImagePath = "Files/nophoto/600x600.png";
            }



            public static class SitePages
            {
                public static class Slider
                {
                    public const string Image = "Files\\SitePages\\Slider\\";
                    public const string ServerImagePath = "Files/SitePages/Slider/";
                    public const string DefaultImagePath = "Files/nophoto/1920x900.png";
                }

                public static class Index
                {
                    public const string Image = "Files\\SitePages\\Index\\";
                    public const string ServerImagePath = "Files/SitePages/Index/";
                    public const string DefaultImagePath = "Files/nophoto/500x600.png";
                }
                public static class About
                {
                    public const string Image = "Files\\SitePages\\About\\";
                    public const string ServerImagePath = "Files/SitePages/About/";
                    public const string DefaultImagePath = "Files/nophoto/800x800.png";
                }
                public static class OutDoorParties
                {
                    public const string Image = "Files\\SitePages\\OutDoorParties\\";
                    public const string ServerImagePath = "Files/SitePages/OutDoorParties/";
                    public const string DefaultImagePath = "Files/nophoto/800x800.png";
                }
                public static class EventHalls
                {
                    public const string Image = "Files\\SitePages\\EventHalls\\";
                    public const string ServerImagePath = "Files/SitePages/EventHalls/";
                    public const string DefaultImagePath = "Files/nophoto/800x800.png";
                }
                public static class Gourmet
                {
                    public const string Image = "Files\\SitePages\\Gourmet\\";
                    public const string ServerImagePath = "Files/SitePages/Gourmet/";
                    public const string DefaultImagePath = "Files/nophoto/600x600.png";
                }
                public static class Contact
                {
                    public const string Image = "Files\\SitePages\\ContactUs\\";
                    public const string ServerImagePath = "Files/SitePages/ContactUs/";
                    public const string DefaultImagePath = "Files/nophoto/800x900.png";
                }
                public static class Banners
                {
                    public const string Image = "Files\\SitePages\\Banners\\";
                    public const string ServerImagePath = "Files/SitePages/Banners/";
                    public const string DefaultImagePath = "Files/nophoto/1920x300.png";
                }
            }
            public static class NoPhotos
            {
                public const string Image = "Files\\nophoto\\";
                public const string ServerImagePath = "Files/nophoto/";
            }


            public static class Announcements
            {
                public const string Image = "Files\\AnnouncmentBanners\\";
                public const string ServerImagePath = "Files/AnnouncmentBanners/";
                public const string DefaultImagePath = "Files/nophoto/1600x300.jpg";
            }
                
        }

        public static class NotificationsNames
        {
            public const string Public = "abp.Public";
            public const string NewContactMsg = "abp.NewContactMsg";
            public const string NewRegisteration = "abp.NewRegisteration";
            public const string QuestionReply = "abp.QuestionReply";
            public const string NewQuestion = "abp.NewQuestion";
            public const string NewEmploymentMsg = "abp.NewEmployment";
            public const string BrancheRequest = "abp.BrancheRequest";
            public const string RejectBrancheRequest = "abp.RejectBrancheRequest";
            public const string NewRequestOrder = "abp.NewRequestOrder";
            public const string NewSubscribePackage = "abp.NewSubscribePackage";
            public const string Logout = "abp.Logout";
            public const string AcceptFuelPriceChangeRequest = "abp.AcceptFuelPriceChangeRequest";
            public const string RefuseFuelPriceChangeRequest = "abp.RefuseFuelPriceChangeRequest";
            public const string NewFuelPriceChangeRequest = "abp.NewFuelPriceChangeRequest";
            public const string VeichleUpdated = "abp.VeichleUpdated";
            public const string FuelTransaction = "abp.FuelTransaction";
            public const string Wallet = "abp.Wallet";
            public const string NewInvoice = "abp.NewInvoice";
            public const string NewVoucher = "abp.NewVoucher";
            public const string NewWalletTransfer = "abp.NewWalletTransfer";
            public const string AcceptWalletTransfer = "abp.AcceptWalletTransfer";
            public const string RefuseWalletTransfer = "abp.RefuseWalletTransfer";
            public const string AcceptSubscriptionTransfer = "abp.AcceptSubscriptionTransfer";
            public const string RefuseSubscriptionTransfer = "abp.RefuseSubscriptionTransfer";
            public const string ExpireSubscription = "abp.ExpireSubscription";
            public const string NewRegisterationRequest = "abp.NewRegisterationRequest";
            public const string SubscribePackageWalletError = "abp.SubscribePackageWalletError";
            public const string UpgradeSubscription = "abp.UpgradeSubscription";
            public const string NewTicket = "abp.NewTicket";
            public const string TicketCompleted = "abp.TicketCompleted";

            
        }

    }
}