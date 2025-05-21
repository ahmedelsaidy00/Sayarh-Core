using Sayarah.AbpZeroTemplate.Auditing.Dto;
using Sayarah.AbpZeroTemplate.Dto;

namespace Sayarah.Application.auditing.Exporting;

public interface IAuditLogListExcelExporter
{
    FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos);

    FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
}
