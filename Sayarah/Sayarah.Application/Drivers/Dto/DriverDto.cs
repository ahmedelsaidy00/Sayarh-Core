using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Drivers;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Drivers.Dto
{
    [AutoMapFrom(typeof(Driver)), AutoMapTo(typeof(Driver))]
    public class DriverDto : FullAuditedEntityDto<long>
    {
        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }

        public long? UserId { get; set; }
        public UserDto User { get; set; }
        public string DriverCode { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath
        {
            get
            {

                string _avatar = User != null ? User.Avatar : Avatar;

                if (!string.IsNullOrEmpty(_avatar) && Utilities.CheckExistImage(6, "600x600_" + _avatar))
                    return FilesPath.Drivers.ServerImagePath + "600x600_" + _avatar;
                else
                    return FilesPath.Drivers.DefaultImagePath;
            }
        }
        public string Licence { get; set; }

        public string FullLicencePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Licence) && Utilities.CheckExistImage(6, "600x600_" + Licence))
                    return FilesPath.Drivers.ServerImagePath + "600x600_" + Licence;
                else
                    return FilesPath.Drivers.DefaultImagePath;
            }
        }

        public DateTime? LicenceExpireDate { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Notes { get; set; }

        public List<SmallDriverVeichleDto> DriverVeichles { get; set; }

        public List<string> PlateNumbers {
            get { 
                List<string> result = new List<string>();
                if(DriverVeichles != null)
                {
                    foreach (var item in DriverVeichles)
                    {
                        if(item.Veichle != null)
                        {
                            result.Add(Utilities.AddSpaces(item.Veichle.FullPlateNumber) + " / " + Utilities.AddSpaces(item.Veichle.FullPlateNumberAr));
                        }
                    }
                }
                return result;
            }
        }

        public int DriverVeichlesCount
        {
            get
            {
                return DriverVeichles != null ? DriverVeichles.Count : 0;
            }
        }

    }

    [AutoMapFrom(typeof(Driver)), AutoMapTo(typeof(Driver))]
    public class ApiDriverDto : EntityDto<long>
    {
        public long? UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string FullFilePath { get; set; }

        public CreateNewUserInput User { get; set; }

    }

    [AutoMapTo(typeof(Driver))]
    public class CreateDriverDto
    {
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? UserId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }

        public string Licence { get; set; }

        public DateTime? LicenceExpireDate { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Notes { get; set; }
        public CreateNewUserInput User { get; set; }
    }


    [AutoMapTo(typeof(Driver))]
    public class UpdateDriverDto : EntityDto<long>
    {
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? UserId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }

        public string Licence { get; set; }

        public DateTime? LicenceExpireDate { get; set; }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Notes { get; set; }
        public CreateNewUserInput User { get; set; }

    }


    public class GetDriversPagedInput : DataTableInputDto
    {
        [DisableAuditing]
        public string Code { get; set; }
        [DisableAuditing]
        public long? CityId { get; set; }
        [DisableAuditing]
        public long? BranchId { get; set; }
        [DisableAuditing]
        public long? CompanyId { get; set; }
        [DisableAuditing]
        public long? Id { get; set; }
        [DisableAuditing]
        public string Name { get; set; }
        [DisableAuditing]
        public string Desc { get; set; }
        [DisableAuditing]
        public bool? IsActive { get; set; }
        [DisableAuditing]
        public string DriverCode { get; set; }
        [DisableAuditing]
        public string BranchCode { get; set; }
        [DisableAuditing]
        public string BranchName { get; set; }
        [DisableAuditing]
        public bool? IsEmployee { get; set; }
        [DisableAuditing]
        public List<long> BranchesIds { get; set; }

        [DisableAuditing]
        public string FullPlateNumber { get; set; }
    }


    public class GetDriversInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public string Name { get; set; }
        [DisableAuditing]
        public string Desc { get; set; }
        public bool? IsActive { get; set; }
        public bool MaxCount { get; set; }
        public string DriverCode { get; set; }
        public List<long> ExcludedList { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }

    [AutoMapFrom(typeof(Driver)), AutoMapTo(typeof(Driver))]
    public class UpdateDriverProfileInput : EntityDto<long>
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
    }

    [AutoMapFrom(typeof(Driver)), AutoMapTo(typeof(Driver))]
    public class UpdateDriverCodeInput
    {
        public long? VeichleId { get; set; }
        public string Code { get; set; }
        public string SimNumber { get; set; }
    }

    public class UpdateDriverCodeOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ApiVeichleDto Veichle { get; set; }
    }



    public class RequestDriversExcelDtoInput : ExcelBaseInput
    {
        public string ExcelTitle { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? DriverId { get; set; }
        public long? VeichleId { get; set; }
        public bool? IsActive { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }


    public class DriverExcelDto  
    {
        public string ExcelTitle { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Branch { get; set; }
        public string VeichlesCount { get; set; }
        public string FullPlateNumbers { get; set; }
        public string Status { get; set; }
    }
}