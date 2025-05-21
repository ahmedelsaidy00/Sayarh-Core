using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Users.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Chips;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.Chips.Dto
{
    [AutoMapFrom(typeof(ChipNumber)), AutoMapTo(typeof(ChipNumber))]
    public class ChipNumberDto : AuditedEntityDto<long>
    {
        public string Code { get; set; }
        public long? ChipDeviceId { get; set; }
        public ChipDeviceDto ChipDevice { get; set; }

        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }

        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }
        public UserDto ActivationUser { get; set; }

        public string CreatorUserName { get; set; }

    }
    [AutoMapFrom(typeof(ChipNumber))]
    public class ApiChipNumberDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? ChipDeviceId { get; set; }
        public ChipDeviceDto ChipDevice { get; set; }
        public long? CompanyId { get; set; }
        public CompanyDto Company { get; set; }

        public long? BranchId { get; set; }
        public BranchDto Branch { get; set; }
        public long? VeichleId { get; set; }
        public VeichleDto Veichle { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }
        public UserDto ActivationUser { get; set; }


    }

    [AutoMapTo(typeof(ChipNumber))]
    public class CreateChipNumberDto
    {
        public string Code { get; set; }
        public long? ChipDeviceId { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }

    }

    [AutoMapTo(typeof(ChipNumber))]
    public class UpdateChipNumberDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? ChipDeviceId { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }


    }
    public class GetChipNumbersInput : DataTableInputDto
    {
        public string Code { get; set; }
        public string ChipDeviceCode { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string VeichleName { get; set; }
        public long? ChipDeviceId { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? VeichleId { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus? Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }

         
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
        public string Reason { get; set; }


    }
    public class GetAllChipNumbers : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? ChipDeviceId { get; set; }
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
        public long? VeichleId { get; set; }
        public string ReleaseNumber { get; set; }
        public ChipStatus? Status { get; set; }
        public DateTime? ActivationDate { get; set; }
        public long? ActivationUserId { get; set; }
         
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
        public string Reason { get; set; }
        public string Lang { get; set; }
        public bool MaxCount { get; set; }
    }

    public class LinkByChipsEmployee
    {
        public long? CompanyId { get; set; }
        public long? BranchId { get; set; }
       public List<string> ChipsNumbers { get; set; }
    }
    public class LinkByChipsEmployeeOutput
    {
        public List<string> LinkedChips { get; set; }
        public List<string> RejectedChips { get; set; }
        public List<string> AcceptedChips { get; set; }

    }

}
