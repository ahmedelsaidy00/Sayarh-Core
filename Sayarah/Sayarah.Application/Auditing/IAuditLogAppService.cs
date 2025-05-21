using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sayarah.AbpZeroTemplate.Auditing.Dto;
using Sayarah.AbpZeroTemplate.Dto;
using Sayarah.Application.auditing.Dto;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.auditing;

public interface IAuditLogAppService : IApplicationService
{
    Task<PagedResultDto<AuditLogListDto>> GetAuditLogs(GetAuditLogsInput input);
    Task<FileDto> GetAuditLogsToExcel(GetAuditLogsInput input);
    Task<PagedResultDto<EntityChangeListDto>> GetEntityChanges(GetEntityChangeInput input);
    Task<PagedResultDto<EntityChangeListDto>> GetEntityTypeChanges(GetEntityTypeChangeInput input);
    Task<FileDto> GetEntityChangesToExcel(GetEntityChangeInput input);
    Task<List<EntityPropertyChangeDto>> GetEntityPropertyChanges(long entityChangeId);
    List<NameValueDto> GetEntityHistoryObjectTypes();
    Task<DataTableOutputDto<AuditLogListDto>> GetPaged(GetAuditLogsInput2 input);
    Task<DataTableOutputDto<EntityChangeListDto>> GetEntityChangesPaged(CustomGetEntityChangeInput input);
    Task<PagedResultDto<EntityChangeWithPropertiesListDto>> GetEntityTypeChangesWithPropertiesChanges(GetEntityTypeChangeInput input);
}