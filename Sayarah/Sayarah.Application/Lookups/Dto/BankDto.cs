using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Lookups;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Lookups.Dto
{
    [AutoMapFrom(typeof(Bank)), AutoMapTo(typeof(Bank))]
    public class BankDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
       
        public string FullFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath) && Utilities.CheckExistImage(14, "300x100_" + FilePath))
                    return FilesPath.Banks.ServerImagePath + "300x100_" + FilePath;
                else
                    return "";
            }
        }

        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد

    }
    [AutoMapFrom(typeof(Bank))]
    public class ApiBankDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string FullFilePath { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد
    }

    [AutoMapTo(typeof(Bank))]
    public class CreateBankDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد
    }

    [AutoMapTo(typeof(Bank))]
    public class UpdateBankDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد
    }
    public class GetBanksInput : DataTableInputDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد
        public bool MatchExact { get; set; }

    }
    public class GetAllBanks : PagedResultRequestDto
    {
        public string Lang { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Iban { get; set; }
        public string AccountNumber { get; set; }// رقم الحساب
        public string BeneficiaryName { get; set; } // اسم المستفيد
        public bool MaxCount { get; set; }
    }
}
