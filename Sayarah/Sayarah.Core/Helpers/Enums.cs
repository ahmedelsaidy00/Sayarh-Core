namespace Sayarah.Core.Helpers
{
    public enum LanguageEnum
    {
        None = 0,
        Ar = 1,
        En = 2
    }

    public enum FileType
    {
        Image = 0,
        Doc = 1,
        Vedio = 2,
        Exel = 3,
        PDF = 4,
        None = 5
    }

    public enum CommitteeAndProgramType
    {
        ExecutiveCommittees = 0,
        AssociationPrograms = 1,

    }
    public enum CancelNote
    {
        Canceled = 0,
        TimeOut = 1,
    }
    public enum PageEnum
    {
        About = 1,
        Contact = 2,
        Intro = 3,
        Terms = 4,
        Privacy = 5,
        UsagePolicy = 6,
        RefundPolicy = 7,
    }
    public enum YouTubeThumbQuality
    {
        Mqdefault = 1,
        Hqdefault = 2,
        Hd720 = 3
    }
    public enum GallaryType
    {
        Gallary = 0,
        About = 1,
        SayarahJourmet = 2,
        OutDoorParties = 3,
        EventHalls = 4,
        Clients = 5

    }

    public enum DeviceType
    {
        Web = 1, Android = 2, IOS = 3
    }
    public enum Gender
    {
        Male = 1, Female = 2
    }
    public enum UserTypes
    {
        Admin = 0,
        Client = 1, 
        Employee = 2,
        Company = 3,
        Branch = 4, 
        Driver = 5, 
        Provider = 6,
        Worker = 7,
        MainProvider = 8,
        AdminEmployee = 9
    }
    public enum OperationType
    {
        Begin = 0,
        Cancel = 1, 
        End = 2,  
    }
    public enum LoginType
    {
        Client = 1, Employee = 2
    }
    public enum MessageType
    {
        Text = 0, Audio = 1, Image = 2, File = 3
    }

    public enum NotificationFilter
    {
        Selected = 1,
        Administrative = 2,
        All = 3
    }

    public enum NotificationType
    {
        Mobile = 1,
        Email = 2,
        Both = 3
    }

    public enum VideoType
    {
        UploadedVideo = 1,
        Youtube = 2
    }
    public enum OrderStatus
    {
        New = 1,
        Accepted = 2,
        Refused = 3,
        Completed = 4
    }
    public enum OrderType
    {
        Done = 1,
        Suspended = 2,
    }
    public enum Packages
    {
        Package1 = 1,
        Package2 = 2,
        Package3 = 3,
        Package4 = 4,
    }
    public enum FilterType
    {
        ByPrice = 1,
        ByRate = 2,
        ByDistance = 3,
    }
    public enum FcmRecieverType
    {
        All = 1,
        Clients = 2,
        Employees = 3
    }
    public enum OrderFilter
    {
        Current = 1,
        Previous = 2,
    }
    public enum TransactionType
    {
        Deposit = 1, // ايداع
        Refund = 2, // سحب 
        SystemCommission = 3, // للنظام
        DepositAdmin = 4, // ايداع
        RefundAdmin = 5, // سحب
        Consumption = 6, // استهلاك
    }
    public enum PayMethod
    {
        Visa = 0,
        Cash = 1,
        Wallet = 2,
        BankTransfer = 3 // تحويل بنكي
    }

    public enum DepositStatus
    {
        Pending = 1,
        Accepted = 2,
        Refused = 3
    }


    public enum NotifyType
    {
        FromCompany = 1,
        BranchTransfer = 2
    }
    

    public enum DiscountEnum
    {
        Without = 0,
        Value = 1,
        Percentage = 2
    }

    public enum ProjectType
    {
        Old = 0,
        Current = 1,
        Soon = 2
    }
    public enum SubjectType
    {
        Volunteer = 0,
        Donations = 1,
        Complaints = 2,
        Initiatives = 3,
    }

    public enum OrderBy
    {
        LowestPrice = 1,
        HighestPrice = 2,
        MostRecent = 3,
        MostOld = 4,
        OlderUsedDate = 5,
        NewerUsedDate = 6,
        LowestDiscount = 7,
        HighestDiscount = 8,
        LowestRate = 9,
        HighestRate = 10
    }
    public enum AddressTypes
    {
        Home = 1, Work = 2, Other = 3
    }
    public enum FilterStatus
    {
        Current = 1,
        Old = 2,
        Paid = 3
    }

    public enum FuelType
    {
        _91 = 1, _95 = 2, diesel = 3
    }

    public enum AccountType
    {
        FuelProvider = 0, Company = 1, CleanProvider = 2, ServicesProvider = 3
    }
    public enum HearAboutUs
    {
        LinkedIn = 0, Facebook = 1,Twitter = 2 , Instagram = 3 , SnapChat = 4 , Influencers = 5 , Other = 6
    }

    
    public enum TransferStatus
    {
        Pending = 0,
        Accepted = 1,
        Refused = 2
    }

    public enum ChangeRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Refused = 2
    }
    public enum RegisterationRequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Refused = 2,
        DuringCreation = 3
    }

    

    public enum JournalType
    {
        Invoice = 1,
        Vouchar = 2
    }

    public enum AccountTypes
    {
        Branch = 1,
        Company = 2,
        Provider = 3,
        MainProvider = 4,
        SayarahApp = 5
    }

    public enum TransTypes
    {
        _1 = 0, _2 = 1, _3 = 2
    }


    public enum DeliveyWays
    {
        Car = 0, Plane = 1
    }

    public enum RequestStatus
    {
        New = 0, Accepted = 1, Rejected = 2
    }




    public enum MessageFrom
    {
        Admin = 0, User = 1
    }

    public enum PackageType
    {
        Monthly = 1,
        Yearly = 2
    }
    public enum SubscriptionTransactionType
    {
        Subscribe = 0,
        Renew = 1,
        Upgrade = 2
    }

    public enum GroupType
    {
        Litre = 0,
        Period = 1,
        Open = 2,
        Trip = 3,
        None = 4
    }

    public enum PeriodConsumptionType
    {
        Money = 0,
        Litre = 1
    }

    //public enum PeriodType
    //{
    //    Daily = 1,
    //    Weekly = 2,
    //    Monthly = 3
    //}

    public enum PeriodType
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }

    public enum VeichleType
    {
        Bus = 0, // حافلة 
        Private = 1, //خصوصي  
        Transport = 2, // نقل
        Taxi = 3, //أجرة 
        Truck = 4, //شاحنة  
        Van = 5, //فان  
        Motorcycle = 6, //دراجة بخارية  
        HeavyEquipment = 7, //معدات ثقيلة  
        PrivateTransfer = 8, //نقل خاص  
        Diplomat = 9, //دبلوماسي 
    }

    public enum ConsumptionType
    {
        Group = 0, // مجموعة 
        Trip = 1   // رحلة  
    }


    public enum TransOutTypes
    {
        Fuel = 1,
        Oil = 2,
        Maintain = 3,
        Wash = 4
    }


    public enum ChipStatus
    {
        Unused = 0,
        Used = 1,
        Linked = 2,
        Blocked = 3,
        Broken = 4,
        Archived = 5
    }


    public enum AnnouncementType
    {
        Without = 1, Advertisement = 2
    }
    public enum AnnouncementUserType
    {
        Company = 1, MainProvider = 2, Branch = 3, Provider = 4
    }


    public enum WalletType
    {
        Fuel = 0,
        Clean = 1,
        Maintain = 2,
    }

    public enum ProblemCategory
    {
        Dashboard = 1,
        Wallet = 2,
        Supscriptions = 3,
        Employees = 4,
        Branches = 5,
        Veichles = 6,
        Drivers = 7,
        Trips = 8,
        VeichlesTransfer = 9,
        Invoices = 10,
        Locations = 11,
        Workers = 12,
        WorkersTransfersRecords = 13,
        FuelPriceChangeRequests = 14,


        Other = 0
    }


    public enum TicketStatus
    {
        Pending = 0,
        Completed = 1
    }

    public enum ServicesTypes
    {
        Market = 1,
        Atm = 2
    }


    public enum TicketFrom
    {
        Company = 0,
        MainProvider = 1,
        Provider = 2,
        Branch = 3,
        Admin = 4
    }


    

}