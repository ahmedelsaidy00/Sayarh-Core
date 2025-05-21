using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Veichles;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(VeichleTransferRecordDriver)) , AutoMapTo(typeof(VeichleTransferRecordDriver))]
    public class VeichleTransferRecordDriverDto : EntityDto<long>
    {
        public long? VeichleTransferRecordId { get; set; }
        public long? DriverId { get; set; }
        public DriverDto Driver { get; set; }

    }

    [AutoMapTo(typeof(VeichleTransferRecordDriver))]
    public class CreateVeichleTransferRecordDriverDto
    {
        public long? VeichleTransferRecordId { get; set; }
        public long? DriverId { get; set; }
    }

     


    [AutoMapTo(typeof(VeichleTransferRecordDriver))]
    public class UpdateVeichleTransferRecordDriverDto : EntityDto<long>
    {
        public long? VeichleTransferRecordId { get; set; }
        public long? DriverId { get; set; }
    }
  
}