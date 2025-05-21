using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(MainProvider)) , AutoMapTo(typeof(MainProvider))]
    public class MainProviderDto : FullAuditedEntityDto<long>
    {
        public long? UserId { get; set; }
        public UserDto User { get; set; }
        public string Code { get; set; }
        public string PhoneNumber
        {
            get
            {
                return User != null ? User.PhoneNumber : string.Empty;
            }
        }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Desc { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }

        public string Avatar { get; set; }
       
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

        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string RegNoFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(RegNoFilePath) && Utilities.CheckExistImage(7, "600x600_" + RegNoFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + RegNoFilePath;
                else
                    return "";
            }
        }


        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string TaxNoFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(TaxNoFilePath) && Utilities.CheckExistImage(7, "600x600_" + TaxNoFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + TaxNoFilePath;
                else
                    return "";
            }
        }

        public string BankName { get; set; }
        public string AccountName { get; set; }

        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }

        public AccountType AccountType { get; set; }


        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public bool AddExternalInvoice { get; set; }
    }



    [AutoMapFrom(typeof(MainProvider)), AutoMapTo(typeof(MainProvider))]
    public class MainProviderBankInfoDto : EntityDto<long>
    {
        
        public string Name { get; set; }
       
        public string Desc { get; set; }
      
        public string Avatar { get; set; }

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

        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string RegNoFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(RegNoFilePath) && Utilities.CheckExistImage(7, "600x600_" + RegNoFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + RegNoFilePath;
                else
                    return "";
            }
        }


        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string TaxNoFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(TaxNoFilePath) && Utilities.CheckExistImage(7, "600x600_" + TaxNoFilePath))
                    return FilesPath.Providers.ServerImagePath + "600x600_" + TaxNoFilePath;
                else
                    return "";
            }
        }

        public string BankName { get; set; }
        public string AccountName { get; set; }
       
    }


    public class MainProviderBankInfoExcelDto  
    {
        public string MainProvider { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankName { get; set; }
        public string AccountName { get; set; }
    }

    [AutoMapFrom(typeof(MainProvider)), AutoMapTo(typeof(MainProvider))]
    public class SmallMainProviderDto : EntityDto<long>
    {
        public long? UserId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }

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
    }



    [AutoMapTo(typeof(MainProvider))]
    public class CreateMainProviderDto
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب



        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
       
        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
       
        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }

        public AccountType AccountType { get; set; }
        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public bool AddExternalInvoice { get; set; }
    }

 
    [AutoMapTo(typeof(MainProvider))]
    public class UpdateMainProviderDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public long UserId { get; set; }
        public CreateNewUserInput User { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب

        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه

        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }

        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }

        public AccountType AccountType { get; set; }
        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public bool AddExternalInvoice { get; set; }
    }



    [AutoMapTo(typeof(MainProvider))]
    public class UpdateMainProviderDocumentsDto : EntityDto<long>
    {
      
        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
    }


    [AutoMapTo(typeof(MainProvider))]
    public class UpdateMainProviderBankInfoDto : EntityDto<long>
    {
       
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BankFilePath { get; set; } // صورة خطاب البنك تكون مصدقه من الغرفة التجاريه
        public string BankName { get; set; }
        public string AccountName { get; set; }
    }


    [AutoMapTo(typeof(MainProvider))]
    public class UpdateMainProviderNationalAddressDto : EntityDto<long>
    {
      
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }
    }



    public class GetMainProvidersPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Desc { get; set; }
        public bool? IsActive { get; set; }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب

        public AccountType? AccountType { get; set; }

        public bool? IsFuel { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }
        public bool? AddExternalInvoice { get; set; }
    }
 
  
    public class GetMainProvidersInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public bool? IsActive { get; set; }
        public bool MaxCount { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public AccountType? AccountType { get; set; }

        public bool? IsFuel { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }
        public bool? AddExternalInvoice { get; set; }

    }


    public class GetMainProvidersExcelInput :  ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public long? MainProviderId { get; set; }
    }

}