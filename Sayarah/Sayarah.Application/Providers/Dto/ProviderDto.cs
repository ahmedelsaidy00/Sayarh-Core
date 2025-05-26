using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users.Dto;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(Provider)), AutoMapTo(typeof(Provider))]
    public class ProviderDto : FullAuditedEntityDto<long>
    {
        public long? UserId { get; set; }
        public UserDto User { get; set; }
        public  long? MainProviderId { get; set; }
        public  MainProviderDto MainProvider { get; set; }

        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Distance { get; set; } 
        public string AddressOnMap { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Avatar { get; set; }
        public string FuelTypes { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        
        public decimal FuelNinetyOnePrice { get; set; }
        public decimal FuelNinetyFivePrice { get; set; }
        public decimal SolarPrice { get; set; }

      

        public decimal InternalWashingPrice { get; set; }
        public decimal ExternalWashingPrice { get; set; }

        public decimal ThreeThousandKiloOilPrice { get; set; }
        public decimal FiveThousandKiloOilPrice { get; set; }
        public decimal TenThousandKiloOilPrice { get; set; }

        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(7, "600x600_" + Avatar))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + Avatar;
                else
                    return FilesPath.Providers.DefaultImagePath;
            }
        }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public string BankFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(BankFilePath) && Utilities.CheckExistImage(7, "600x600_" + BankFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + BankFilePath;
                else
                    return "";
            }
        }

        public string BankName { get; set; }
        public string AccountName { get; set; }


        public int WorkersCount { get; set; } // عدد العمال
        public string ManagerName { get; set; }

        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Services { get; set; }
        public bool AddExternalInvoice { get; set; }

        public bool VisibleInMap { get; set; }
    }


    [AutoMapFrom(typeof(Provider)), AutoMapTo(typeof(Provider))]
    public class SmallProviderDto : EntityDto<long>
    {
        public long? MainProviderId { get; set; }
        public SmallMainProviderDto MainProvider { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

    }

    [AutoMapFrom(typeof(Provider)), AutoMapTo(typeof(Provider))]
    public class ApiProviderDto : EntityDto<long>
    { 
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }

        public long? MainProviderId { get; set; }
        public MainProviderDto MainProvider { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double Distance { get; set; }
        public string AddressOnMap { get; set; }
        public string Avatar { get; set; }
        public string FuelTypes { get; set; } 
        public string[] FinalFuelTypes { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        
        public decimal FuelNinetyOnePrice { get; set; }
        public decimal FuelNinetyFivePrice { get; set; }
        public decimal SolarPrice { get; set; }

      

        public decimal InternalWashingPrice { get; set; }
        public decimal ExternalWashingPrice { get; set; }

        public decimal ThreeThousandKiloOilPrice { get; set; }
        public decimal FiveThousandKiloOilPrice { get; set; }
        public decimal TenThousandKiloOilPrice { get; set; }

        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(7, "600x600_" + Avatar))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + Avatar;
                else
                    return FilesPath.Providers.DefaultImagePath;
            }
        }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب

        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public string BankFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(BankFilePath) && Utilities.CheckExistImage(7, "600x600_" + BankFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + BankFilePath;
                else
                    return "";
            }
        }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public int WorkersCount { get; set; } // عدد العمال
        public string ManagerName { get; set; }

        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Services { get; set; }

        public List<string> ServicesList { get; set; }
        
    }


    [AutoMapFrom(typeof(Provider)), AutoMapTo(typeof(Provider))]
    public class PlainProviderDto : EntityDto<long>
    {
        public long? MainProviderId { get; set; }
        public string MainProviderNameAr { get; set; }
        public string NameAr { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressOnMap { get; set; }
        public string Avatar { get; set; }
        public string FuelTypes { get; set; }
        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public decimal FuelNinetyOnePrice { get; set; }
        public decimal FuelNinetyFivePrice { get; set; }
        public decimal SolarPrice { get; set; }
        public string FullFilePath { get; set; }
        public int WorkersCount { get; set; } // عدد العمال
        public string ManagerName { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Services { get; set; }
    }




    [AutoMapTo(typeof(Provider))]
    public class CreateProviderDto
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressOnMap { get; set; }
        public string Avatar { get; set; }
        public string FuelTypes { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        
        public decimal FuelNinetyOnePrice { get; set; }
        public decimal FuelNinetyFivePrice { get; set; }
        public decimal SolarPrice { get; set; }

        

        public decimal InternalWashingPrice { get; set; }
        public decimal ExternalWashingPrice { get; set; }

        public decimal ThreeThousandKiloOilPrice { get; set; }
        public decimal FiveThousandKiloOilPrice { get; set; }
        public decimal TenThousandKiloOilPrice { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public int WorkersCount { get; set; } // عدد العمال
        public string ManagerName { get; set; }


        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Street { get; set; }

        public string Services { get; set; }

        public bool? IsEmployee { get; set; }

        public bool AddExternalInvoice { get; set; }
    }


    [AutoMapTo(typeof(Provider))]
    public class UpdateProviderDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? MainProviderId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressOnMap { get; set; }
        public string Avatar { get; set; }
        public string FuelTypes { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsOil { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        
        public decimal FuelNinetyOnePrice { get; set; }
        public decimal FuelNinetyFivePrice { get; set; }
        public decimal SolarPrice { get; set; }

      

        public decimal InternalWashingPrice { get; set; }
        public decimal ExternalWashingPrice { get; set; }

        public decimal ThreeThousandKiloOilPrice { get; set; }
        public decimal FiveThousandKiloOilPrice { get; set; }
        public decimal TenThousandKiloOilPrice { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public int WorkersCount { get; set; } // عدد العمال
        public string ManagerName { get; set; }

        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Services { get; set; }
        public bool AddExternalInvoice { get; set; }
    }


    public class GetProvidersPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string ManagerName { get; set; }
        public string FuelTypes { get; set; } 
        public bool? IsFuel { get; set; }
        public bool? IsOil { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }
        public bool? IsActive { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }

        public bool? AddExternalInvoice { get; set; }
    }


    public class GetProvidersInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public long? EmployeeId { get; set; }
        public string Name { get; set; }
        public string FuelTypes { get; set; } 
        public bool? IsFuel { get; set; }
        public bool? IsOil { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }
        public bool? IsActive { get; set; }
        public bool MaxCount { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankName { get; set; }
        public string AccountName { get; set; }

        public List<long> BranchesIds { get; set; }
        public bool? AddExternalInvoice { get; set; }
        public bool ShowDeleted { get; set; }

        public bool? VisibleInMap { get; set; }
    }
    public class GetProvidersInputApi : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressOnMap { get; set; }
        public double Distance { get; set; }
        public string FuelTypes { get; set; } 
        public bool? IsFuel { get; set; }
        public bool? IsOil { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }
       
        public decimal? FuelNinetyOnePrice { get; set; }
        public decimal? FuelNinetyFivePrice { get; set; }
        public decimal? SolarPrice { get; set; }

      
        public decimal? InternalWashingPrice { get; set; }
        public decimal? ExternalWashingPrice { get; set; }

        public decimal? ThreeThousandKiloOilPrice { get; set; }
        public decimal? FiveThousandKiloOilPrice { get; set; }
        public decimal? TenThousandKiloOilPrice { get; set; }

        public bool? IsActive { get; set; }
        public bool? CalculateDistance { get; set; }
        public bool MaxCount { get; set; }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public bool? VisibleInMap { get; set; }

    }

    public class GetProvidersByMainProviderIdInputApi : PagedResultRequestDto
    {
        public long? UserId { get; set; }
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressOnMap { get; set; }
        public double Distance { get; set; }
        public string FuelTypes { get; set; }
        public bool? IsFuel { get; set; }
        public bool? IsOil { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }

        public decimal? FuelNinetyOnePrice { get; set; }
        public decimal? FuelNinetyFivePrice { get; set; }
        public decimal? SolarPrice { get; set; }


        public decimal? InternalWashingPrice { get; set; }
        public decimal? ExternalWashingPrice { get; set; }

        public decimal? ThreeThousandKiloOilPrice { get; set; }
        public decimal? FiveThousandKiloOilPrice { get; set; }
        public decimal? TenThousandKiloOilPrice { get; set; }

        public bool? IsActive { get; set; }
        public bool? CalculateDistance { get; set; }
        public bool MaxCount { get; set; }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public bool? VisibleInMap { get; set; }

    }

}