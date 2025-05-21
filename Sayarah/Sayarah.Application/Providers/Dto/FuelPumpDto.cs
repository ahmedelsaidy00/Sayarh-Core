using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Providers;
using System.Collections.Generic;
namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(FuelPump)), AutoMapTo(typeof(FuelPump))]
    public class FuelPumpDto : FullAuditedEntityDto<long>
    {
        public long? ProviderId { get; set; }
        public ProviderDto Provider { get; set; }

        public string Code { get; set; }
        public string QrCode { get; set; }

    }
    [AutoMapFrom(typeof(FuelPump)), AutoMapTo(typeof(FuelPump))]
    public class ApiFuelPumpDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public string Code { get; set; }
       
    }


    [AutoMapTo(typeof(FuelPump))]
    public class CreateFuelPumpDto
    {
        public long? ProviderId { get; set; }
        public string Code { get; set; }
        public string QrCode { get; set; }
    }
    public class CreateMultipleFuelPumpDto
    {
        public long? ProviderId { get; set; }
        public int Count { get; set; }
    }


    [AutoMapTo(typeof(FuelPump))]
    public class UpdateFuelPumpDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public string Code { get; set; }
        public string QrCode { get; set; }
    }


    public class GetFuelPumpsPagedInput : DataTableInputDto
    {
        public long? Id { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public string Name { get; set; }

        
        public string Code { get; set; }
        public string QrCode { get; set; }

    }


    public class GetFuelPumpsInput : PagedResultRequestDto
    {
        public long? Id { get; set; }
        public long? MainProviderId { get; set; }
        public long? ProviderId { get; set; }
        public long? EmployeeId { get; set; }
        public string Name { get; set; }
        public string QrCode { get; set; }
        public bool MaxCount { get; set; }
    }


    public class GenerateQrCodeList 
    {
        public List<long> Ids { get; set; }
    }

    public class GenerateQrCodeListOutput
    {
        public string  Code{ get; set; }
        public string  QrCode { get; set; }
    }

}