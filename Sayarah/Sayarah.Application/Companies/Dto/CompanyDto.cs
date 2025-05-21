using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Companies;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Companies.Dto
{
    [AutoMapFrom(typeof(Company)) , AutoMapTo(typeof(Company))]
    public class CompanyDto : FullAuditedEntityDto<long>
    {
        public long? UserId { get; set; }
        public UserDto User { get; set; }
        public string PhoneNumber { get{
                return User != null ? User.PhoneNumber : string.Empty;
            }
        }

        public string Code { get; set; }
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
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(4, "600x600_" + Avatar))
                    return FilesPath.Companies.ServerImagePath + "600x600_" + Avatar;
                else
                    return FilesPath.Companies.DefaultImagePath;
            }
        }
        public long? CompanyTypeId { get; set; }
        public CompanyTypeDto CompanyType { get; set; }
        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string RegNoFullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(RegNoFilePath) && Utilities.CheckExistImage(4, "600x600_" + RegNoFilePath))
                    return FilesPath.Companies.ServerImagePath + "600x600_" + RegNoFilePath;
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
                if (!string.IsNullOrEmpty(TaxNoFilePath) && Utilities.CheckExistImage(4, "600x600_" + TaxNoFilePath))
                    return FilesPath.Companies.ServerImagePath + "600x600_" + TaxNoFilePath;
                else
                    return "";
            }
        }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }
        public bool CounterPicIsRequired { get; set; } // صورة العداد إلزامية ؟
        public bool ActivateTimeBetweenFuelTransaction { get; set; }
        public int TimeBetweenFuelTransaction { get; set; }

    }


    [AutoMapFrom(typeof(Company)), AutoMapTo(typeof(Company))]
    public class SmallCompanyDto :EntityDto<long>
    {
        public long? UserId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(4, "600x600_" + Avatar))
                    return FilesPath.Companies.ServerImagePath + "600x600_" + Avatar;
                else
                    return FilesPath.Companies.DefaultImagePath;
            }
        }
        public bool CounterPicIsRequired { get; set; }  
    }

    [AutoMapTo(typeof(Company))]
    public class CreateCompanyDto
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
        public long? CompanyTypeId { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }
        public bool CounterPicIsRequired { get; set; }
        public bool ActivateTimeBetweenFuelTransaction { get; set; }
        public int TimeBetweenFuelTransaction { get; set; }
    }

 
    [AutoMapTo(typeof(Company))]
    public class UpdateCompanyDto : EntityDto<long>
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
        public long? CompanyTypeId { get; set; } 
        public bool IsFuel { get; set; }
        public bool IsClean { get; set; }
        public bool IsMaintain { get; set; }
        public string RegNo { get; set; }
        public string RegNoFilePath { get; set; }
        public string TaxNo { get; set; }
        public string TaxNoFilePath { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string BoxNumber { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string AdditionalCode { get; set; }
        public string UnitNumber { get; set; }
        public bool CounterPicIsRequired { get; set; }
        public bool ActivateTimeBetweenFuelTransaction { get; set; }
        public int TimeBetweenFuelTransaction { get; set; }
    }

     
    public class GetCompaniesPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Desc { get; set; }
        public bool? IsActive { get; set; }
        public long? CompanyTypeId { get; set; } 
        public string RegNo { get; set; } // رقم السجل التجاري
        public string RegNoFilePath { get; set; }
        public string TaxNo { get; set; } // الرقم الضريبي
        public string TaxNoFilePath { get; set; }
        public bool? IsFuel { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }

    }
 
    public class GetCompaniesInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public bool? IsActive { get; set; }
        public bool MaxCount { get; set; }
        public bool? IsFuel { get; set; }
        public bool? IsClean { get; set; }
        public bool? IsMaintain { get; set; }

    }

    [AutoMapFrom(typeof(Company)), AutoMapTo(typeof(Company))]
    public class GetWalletDetailsDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public decimal? WalletAmount { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Avatar) && Utilities.CheckExistImage(4, "600x600_" + Avatar))
                    return FilesPath.Companies.ServerImagePath + "600x600_" + Avatar;
                else
                    return FilesPath.Companies.DefaultImagePath;
            }
        }
        public DateTime? LastDepositCreationTime { get; set; }

    }

    public class ChangeEmailAndPhone : EntityDto<long>
    {
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ConfirmationCode { get; set; }
    }

    public class CompanyNameDto : EntityDto<long>
    {
        public string Name { get; set; }
    }


}