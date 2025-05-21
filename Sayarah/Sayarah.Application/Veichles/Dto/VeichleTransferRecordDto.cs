using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sayarah.Application.Veichles.Dto
{
    [AutoMapFrom(typeof(VeichleTransferRecord)) , AutoMapTo(typeof(VeichleTransferRecord))]
    public class VeichleTransferRecordDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
               
        public long? SourceBranchId { get; set; }
        public ApiBranchDto SourceBranch { get; set; }
               
        public long? TargetBranchId { get; set; }
        public ApiBranchDto TargetBranch { get; set; }
               
        public long? VeichleId { get; set; }
        public ApiVeichleDto Veichle { get; set; }
               
        public TransferStatus? Status { get; set; }

        public string CreatorUserName { get; set; }

        public bool TransferDrivers { get; set; }
        public List<VeichleTransferRecordDriverDto> VeichleTransferRecordDrivers { get; set; }
    }

    [AutoMapTo(typeof(VeichleTransferRecord))]
    public class CreateVeichleTransferRecordDto
    {
        public string Code { get; set; }
        public long? SourceBranchId { get; set; }
        public long? TargetBranchId { get; set; }
        public long? VeichleId { get; set; }
        public TransferStatus? Status { get; set; }

        public bool TransferDrivers { get; set; }
        public List<VeichleTransferRecordDriverDto> VeichleTransferRecordDrivers { get; set; }
    }


    public class ManageTransfersInput
    {
       // public List<long> VeichlesId { get; set; }
        public List<TransferViechle> Veichles { get; set; }
        public long? TargetBranchId { get; set; }
    }

    public class TransferViechle : EntityDto<long>
    {
        public long? DriverId { get; set; }
        public bool TransferDrivers { get; set; }

        public List<TransferDriverVeichle> DriverVeichles { get; set; }
    }

    public class TransferDriverVeichle : EntityDto<long>
    {
        public long? DriverId { get; set; }
        public bool IsSelected { get; set; }
    }


    [AutoMapTo(typeof(VeichleTransferRecord))]
    public class UpdateVeichleTransferRecordDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? SourceBranchId { get; set; }
        public long? TargetBranchId { get; set; }
        public long? VeichleId { get; set; }
        public TransferStatus? Status { get; set; }
    }

    public class GetVeichleTransferRecordsPagedInput : DataTableInputDto
    {
        public string Code { get; set; }
        public string SourceBranchName { get; set; }
        public string TargetBranchName { get; set; }
        public string VeichleName { get; set; }

        public long? SourceBranchId { get; set; }
        public long? TargetBranchId { get; set; }
        public long? VeichleId { get; set; }
        public TransferStatus? Status { get; set; }

        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
       
        public string Name { get; set; }

        public bool? IsEmployee { get; set; }
        public List<long> BranchesIds { get; set; }
    }
 
  
    public class GetVeichleTransferRecordsInput : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? SourceBranchId { get; set; }
        public long? TargetBranchId { get; set; }
        public long? VeichleId { get; set; }
        public TransferStatus? Status { get; set; }

        public long? Id { get; set; }
        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }

    }
}