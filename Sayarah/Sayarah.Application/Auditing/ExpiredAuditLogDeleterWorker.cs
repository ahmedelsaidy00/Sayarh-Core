using Abp.Auditing;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Logging;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using Sayarah.AbpZeroTemplate.Auditing;
using Sayarah.MultiTenancy;
using System.Linq.Expressions;

namespace Sayarah.Application.auditing;

public class ExpiredAuditLogDeleterWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
{
    public bool IsEnabled { get; }
    private const int CheckPeriodAsMilliseconds = 1 * 1000 * 60 * 3; // 3min
    private const int MaxDeletionCount = 10000;
    private readonly TimeSpan _logExpireTime = TimeSpan.FromDays(7);
    private readonly IRepository<AuditLog, long> _auditLogRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IExpiredAndDeletedAuditLogBackupService _expiredAndDeletedAuditLogBackupService;

    public ExpiredAuditLogDeleterWorker(
        AbpTimer timer,
        IRepository<AuditLog, long> auditLogRepository,
        IRepository<Tenant> tenantRepository,
        IExpiredAndDeletedAuditLogBackupService expiredAndDeletedAuditLogBackupService) : base(timer)
    {
        _auditLogRepository = auditLogRepository;
        _tenantRepository = tenantRepository;
        _expiredAndDeletedAuditLogBackupService = expiredAndDeletedAuditLogBackupService;

        LocalizationSourceName = SayarahConsts.LocalizationSourceName;

        Timer.Period = CheckPeriodAsMilliseconds;
        Timer.RunOnStart = true;
    }

    protected override void DoWork()
    {
        if (!IsEnabled)
        {
            return;
        }

        var expireDate = Clock.Now - _logExpireTime;

        List<int> tenantIds;
        using (var uow = UnitOfWorkManager.Begin())
        {
            tenantIds = _tenantRepository.GetAll()
                .Where(t => !string.IsNullOrEmpty(t.ConnectionString))
                .Select(t => t.Id)
                .ToList();

            uow.Complete();
        }

        DeleteAuditLogsOnHostDatabase(expireDate);

        foreach (var tenantId in tenantIds)
        {
            DeleteAuditLogsOnTenantDatabase(tenantId, expireDate);
        }
    }
    protected virtual void DeleteAuditLogsOnHostDatabase(DateTime expireDate)
    {
        try
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                    {
                        DeleteAuditLogs(expireDate);
                        uow.Complete();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogSeverity.Error, $"An error occured while deleting audit logs on host database", e);
        }
    }
    protected virtual void DeleteAuditLogsOnTenantDatabase(int tenantId, DateTime expireDate)
    {
        try
        {
            using var uow = UnitOfWorkManager.Begin();
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DeleteAuditLogs(expireDate);
                uow.Complete();
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogSeverity.Error,
                $"An error occured while deleting audit log for tenant. TenantId: {tenantId}", e);
        }
    }
    private void DeleteAuditLogs(DateTime expireDate)
    {
        var expiredEntryCount = _auditLogRepository.LongCount(l => l.ExecutionTime < expireDate);

        if (expiredEntryCount == 0)
        {
            return;
        }

        static void BatchDelete(Expression<Func<AuditLog, bool>> expression)
        {
            //if (_expiredAndDeletedAuditLogBackupService.CanBackup())
            //{
            //    var auditLogs = _auditLogRepository.GetAll().AsNoTracking().Where(expression).ToList();
            //    _expiredAndDeletedAuditLogBackupService.Backup(auditLogs);
            //}

            ////will not delete the logs from database if backup operation throws an exception
            //AsyncHelper.RunSync(() => _auditLogRepository.BatchDeleteAsync(expression));
        }

        if (expiredEntryCount > MaxDeletionCount)
        {
            var deleteStartId = _auditLogRepository.GetAll().OrderBy(l => l.Id).Skip(MaxDeletionCount)
                .Select(x => x.Id).First();

            BatchDelete(l => l.Id < deleteStartId);
        }
        else
        {
            BatchDelete(l => l.ExecutionTime < expireDate);
        }
    }
}