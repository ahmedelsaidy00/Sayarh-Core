using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Core.Helpers;
using Sayarah.Providers;

namespace Sayarah.Application.Providers.Dto
{
    [AutoMapFrom(typeof(WorkerTransferRecord)) , AutoMapTo(typeof(WorkerTransferRecord))]
    public class WorkerTransferRecordDto : FullAuditedEntityDto<long>
    {
        public string Code { get; set; }
               
        public long? SourceProviderId { get; set; }
        public ApiProviderDto SourceProvider { get; set; }
               
        public long? TargetProviderId { get; set; }
        public ApiProviderDto TargetProvider { get; set; }
               
        public long? WorkerId { get; set; }
        public ApiWorkerDto Worker { get; set; }
               
        public TransferStatus? Status { get; set; }

        public string CreatorUserName { get; set; }
        
    }

    [AutoMapTo(typeof(WorkerTransferRecord))]
    public class CreateWorkerTransferRecordDto
    {
        public string Code { get; set; }
        public long? SourceProviderId { get; set; }
        public long? TargetProviderId { get; set; }
        public long? WorkerId { get; set; }
        public TransferStatus? Status { get; set; }
    }


    public class ManageTransfersInput
    {
        public List<long> WorkersId { get; set; }
        public long? TargetProviderId { get; set; }
    }


    [AutoMapTo(typeof(WorkerTransferRecord))]
    public class UpdateWorkerTransferRecordDto : EntityDto<long>
    {
        public string Code { get; set; }
        public long? SourceProviderId { get; set; }
        public long? TargetProviderId { get; set; }
        public long? WorkerId { get; set; }
        public TransferStatus? Status { get; set; }
    }

    public class GetWorkerTransferRecordsPagedInput : DataTableInputDto
    {
        public string Code { get; set; }
        public string SourceProviderName { get; set; }
        public string TargetProviderName { get; set; }
        public string WorkerName { get; set; }

        public long? SourceProviderId { get; set; }
        public long? TargetProviderId { get; set; }
        public long? WorkerId { get; set; }
        public TransferStatus? Status { get; set; }

        public long? Id { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
       
        public string Name { get; set; }
     
    }
 
  
    public class GetWorkerTransferRecordsInput : PagedResultRequestDto
    {
        public string Code { get; set; }
        public long? SourceProviderId { get; set; }
        public long? TargetProviderId { get; set; }
        public long? WorkerId { get; set; }
        public TransferStatus? Status { get; set; }

        public long? Id { get; set; }
        public long? ProviderId { get; set; }
        
        public long? MainProviderId { get; set; }
        public string Name { get; set; }
        public bool MaxCount { get; set; }

    }
}