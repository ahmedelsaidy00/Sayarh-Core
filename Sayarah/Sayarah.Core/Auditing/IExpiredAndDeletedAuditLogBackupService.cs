using Abp.Auditing;
using System.Collections.Generic;

namespace Sayarah.AbpZeroTemplate.Auditing
{
    public interface IExpiredAndDeletedAuditLogBackupService
    {
        // bool CanBackup();

        void Backup(List<AuditLog> auditLogs);
    }
}