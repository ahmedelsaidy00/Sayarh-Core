using Abp.Dependency;
using Abp.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Sayarah.AbpZeroTemplate.Auditing.Dto;
using Sayarah.AbpZeroTemplate.Dto;
using Sayarah.AbpZeroTemplate.Storage;
using Sayarah.Application.DataExporting.Excel.MiniExcel;

namespace Sayarah.Application.auditing.Exporting;

public class AuditLogListExcelExporter(
    ITimeZoneConverter timeZoneConverter,
    IAbpSession abpSession,
    ILocalizationManager localizationManager,
    ITempFileCacheManager tempFileCacheManager) : MiniExcelExcelExporterBase(tempFileCacheManager), IAuditLogListExcelExporter, ITransientDependency
{
    private readonly ITimeZoneConverter _timeZoneConverter = timeZoneConverter;
    private readonly IAbpSession _abpSession = abpSession;
    private readonly ILocalizationManager _localizationManager = localizationManager;

    public FileDto ExportToFile(List<AuditLogListDto> auditLogList)
    {
        var items = auditLogList.Select(auditLog => new Dictionary<string, object>
        {
            { L("Time"), _timeZoneConverter.Convert(auditLog.ExecutionTime, _abpSession.TenantId, _abpSession.GetUserId()) },
            { L("UserName"), auditLog.UserName },
            { L("Service"), auditLog.ServiceName },
            { L("Action"), auditLog.MethodName },
            { L("Parameters"), auditLog.Parameters },
            { L("Duration"), auditLog.ExecutionDuration },
            { L("IpAddress"), auditLog.ClientIpAddress },
            { L("Client"), auditLog.ClientName },
            { L("Browser"), auditLog.BrowserInfo },
            { L("ErrorState"), auditLog.Exception.IsNullOrEmpty() ? L("Success") : auditLog.Exception },
        }).ToList();

        return CreateExcelPackage("AuditLogs.xlsx", items);
    }

    public FileDto ExportToFile(List<EntityChangeListDto> entityChangeList)
    {
        var items = entityChangeList.Select(entityChange => new Dictionary<string, object>
        {
            { L("Action"), entityChange.ChangeType.ToString() },
            { L("Object"), entityChange.EntityTypeFullName },
            { L("UserName"), entityChange.UserName },
            { L("Time"), _timeZoneConverter.Convert(entityChange.ChangeTime, _abpSession.TenantId, _abpSession.GetUserId()) },
        }).ToList();

        return CreateExcelPackage("DetailedLogs.xlsx", items);
    }

    private string L(string name)
    {
        return _localizationManager.GetString("AbpZeroTemplate", name);
    }
}
