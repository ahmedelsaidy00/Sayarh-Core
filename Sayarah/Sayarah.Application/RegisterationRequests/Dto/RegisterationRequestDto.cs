using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Lookups.Dto;
using Sayarah.Core.Helpers;
using Sayarah.RegisterationRequests;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.RegisterationRequests.Dto
{
    [AutoMapFrom(typeof(RegisterationRequest)), AutoMapTo(typeof(RegisterationRequest))]
    public class RegisterationRequestDto : FullAuditedEntityDto<long>
    {
        public AccountType AccountType { get; set; }
        public long? AccountId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberConfirmationCode { get; set; }

        public string EmailAddress { get; set; }
        public string EmailAddressConfirmationCode { get; set; }

        public bool PhoneNumberConfirmed { get; set; }
        public bool EmailAddressConfirmed { get; set; }

        public string Password { get; set; }
        public HearAboutUs HearAboutUs { get; set; }

        public RegisterationRequestStatus Status { get; set; }

        public string RefuseReason { get; set; }
        public string FilePath { get; set; }
        public string FullFilePath
        {
            get
            {
                int pathKey = AccountType == AccountType.Company ? 4 : 7 ;
                string serverImagePath = AccountType == AccountType.Company ? FilesPath.Companies.ServerImagePath : FilesPath.Providers.ServerImagePath;
                
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(pathKey, FilePath))
                    return serverImagePath + FilePath;
                else
                    return AccountType == AccountType.Company ? FilesPath.Companies.DefaultImagePath : FilesPath.Providers.DefaultImagePath;
            }
        }

        public string UserName { get; set; }

        public long? CompanyTypeId { get; set; }
        public CompanyTypeDto CompanyType { get; set; }


        public string RegNo { get; set; } // رقم السجل التجاري
        public string RegNoFilePath { get; set; }

        public string FullRegNoFilePath
        {
            get
            {
                int pathKey = AccountType == AccountType.Company ? 4 : 7;
                string serverImagePath = AccountType == AccountType.Company ? FilesPath.Companies.ServerImagePath : FilesPath.Providers.ServerImagePath;

                if (!string.IsNullOrEmpty(RegNoFilePath) && Utilities.CheckExistImage(pathKey, RegNoFilePath))
                    return serverImagePath + RegNoFilePath;
                else
                    return AccountType == AccountType.Company ? FilesPath.Companies.DefaultImagePath : FilesPath.Providers.DefaultImagePath;
            }
        }


        public string TaxNo { get; set; } // الرقم الضريبي
        public string TaxNoFilePath { get; set; }

        public string FullTaxNoFilePath
        {
            get
            {
                int pathKey = AccountType == AccountType.Company ? 4 : 7;
                string serverImagePath = AccountType == AccountType.Company ? FilesPath.Companies.ServerImagePath : FilesPath.Providers.ServerImagePath;

                if (!string.IsNullOrEmpty(TaxNoFilePath) && Utilities.CheckExistImage(pathKey, TaxNoFilePath))
                    return serverImagePath + TaxNoFilePath;
                else
                    return AccountType == AccountType.Company ? FilesPath.Companies.DefaultImagePath : FilesPath.Providers.DefaultImagePath;
            }
        }


    }
   

    [AutoMapTo(typeof(RegisterationRequest))]
    public class CreateRegisterationRequestDto
    {
        public AccountType AccountType { get; set; }
        public long? AccountId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberConfirmationCode { get; set; }

        public string EmailAddress { get; set; }
        public string EmailAddressConfirmationCode { get; set; }

        public bool PhoneNumberConfirmed { get; set; }
        public bool EmailAddressConfirmed { get; set; }

        public string Password { get; set; }
        public HearAboutUs HearAboutUs { get; set; }

        public RegisterationRequestStatus Status { get; set; }

        public string RefuseReason { get; set; }
        public string FilePath { get; set; }

        public string UserName { get; set; }

        public long? CompanyTypeId { get; set; }
        
        public string RegNo { get; set; } // رقم السجل التجاري
        public string RegNoFilePath { get; set; }
      
        public string TaxNo { get; set; } // الرقم الضريبي
        public string TaxNoFilePath { get; set; }

     

    }


    [AutoMapTo(typeof(RegisterationRequest))]
    public class UpdateRegisterationRequestDto : EntityDto<long>
    {
        public AccountType AccountType { get; set; }
        public long? AccountId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberConfirmationCode { get; set; }

        public string EmailAddress { get; set; }
        public string EmailAddressConfirmationCode { get; set; }

        public bool PhoneNumberConfirmed { get; set; }
        public bool EmailAddressConfirmed { get; set; }

        public string Password { get; set; }
        public HearAboutUs HearAboutUs { get; set; }

        public RegisterationRequestStatus Status { get; set; }

        public string RefuseReason { get; set; }
        public string FilePath { get; set; }

        public string UserName { get; set; }

        public long? CompanyTypeId { get; set; }

        public string RegNo { get; set; } // رقم السجل التجاري
        public string RegNoFilePath { get; set; }

        public string TaxNo { get; set; } // الرقم الضريبي
        public string TaxNoFilePath { get; set; }


    }



    public class GetRegisterationRequestsPagedInput : DataTableInputDto
    {

        public long? Id { get; set; }
        public AccountType? AccountType { get; set; }
        public long? AccountId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberConfirmationCode { get; set; }

        public string EmailAddress { get; set; }
        public string EmailAddressConfirmationCode { get; set; }

        public bool? PhoneNumberConfirmed { get; set; }
        public bool? EmailAddressConfirmed { get; set; }

        public string Password { get; set; }
        public HearAboutUs? HearAboutUs { get; set; }

        public RegisterationRequestStatus? Status { get; set; }

        public string RefuseReason { get; set; }
        public string FilePath { get; set; }
        public string Name { get; set; }


        public string UserName { get; set; }

        public long? CompanyTypeId { get; set; }

        public string RegNo { get; set; } // رقم السجل التجاري
        public string RegNoFilePath { get; set; }

        public string TaxNo { get; set; } // الرقم الضريبي
        public string TaxNoFilePath { get; set; }



    }


    public class GetRegisterationRequestsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public AccountType? AccountType { get; set; }
        public long? AccountId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescAr { get; set; }
        public string DescEn { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberConfirmationCode { get; set; }

        public string EmailAddress { get; set; }
        public string EmailAddressConfirmationCode { get; set; }

        public bool? PhoneNumberConfirmed { get; set; }
        public bool? EmailAddressConfirmed { get; set; }

        public string Password { get; set; }
        public HearAboutUs? HearAboutUs { get; set; }

        public RegisterationRequestStatus? Status { get; set; }

        public string RefuseReason { get; set; }
        public string FilePath { get; set; }
        public bool MaxCount { get; set; }
    }
}