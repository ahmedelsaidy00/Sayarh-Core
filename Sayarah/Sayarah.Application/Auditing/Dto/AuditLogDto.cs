using Abp.Events.Bus.Entities;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.auditing.Dto;
public class GetAuditLogsInput2 :  DataTableInputDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string UserName { get; set; }
    public string ServiceName { get; set; }
    public string MethodName { get; set; }
    public string BrowserInfo { get; set; }
    public bool? HasException { get; set; }
    public int? MinExecutionDuration { get; set; }
    public int? MaxExecutionDuration { get; set; }
    public int? TenantId { get; set; }
    public long? UserId { get; set; }
    public string Parameters { get; set; }
    public string ReturnValue { get; set; }
    public DateTime? ExecutionTime { get; set; }
    public int? ExecutionDuration { get; set; }
    public string ClientIpAddress { get; set; }
    public string ClientName { get; set; }
    public string ExceptionMessage { get; set; }
    public string Exception { get; set; }
    public long? ImpersonatorUserId { get; set; }
    public int? ImpersonatorTenantId { get; set; }
    public string CustomData { get; set; }
}

public class CustomGetEntityChangeInput : DataTableInputDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string EntityId { get; set; }
    public string UserName { get; set; }
    public string EntityTypeFullName { get; set; }
    public EntityChangeType? ChangeType { get; set; }
}
