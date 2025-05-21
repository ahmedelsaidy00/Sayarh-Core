using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Chips;

namespace Sayarah.Application.Chips.Dto
{
    [AutoMapFrom(typeof(ChipDevice)), AutoMapTo(typeof(ChipDevice))]
    public class ChipDeviceDto : AuditedEntityDto<long>
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }
        public string CreatorUserName { get; set; }

    }
    [AutoMapFrom(typeof(ChipDevice))]
    public class ApiChipDeviceDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }

    }

    [AutoMapTo(typeof(ChipDevice))]
    public class CreateChipDeviceDto
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }

    }

    [AutoMapTo(typeof(ChipDevice))]
    public class UpdateChipDeviceDto : EntityDto<long>
    {
        public string Code { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }

    }
    public class GetChipDevicesInput : DataTableInputDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
        public string Reason { get; set; }


    }
    public class GetAllChipDevices : PagedResultRequestDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
        public string Reason { get; set; }
        public string Lang { get; set; }
        public bool MaxCount { get; set; }
    }
}
