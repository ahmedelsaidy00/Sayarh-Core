using Abp.Application.Services;
using Abp.Notifications;
using System;
using System.Threading.Tasks;


namespace Sayarah.Application.Helpers.NotificationService
{
    public interface IAbpNotificationHelper : IApplicationService
    {
        Task<GetNotificationsOutput> GetUserAbpNotifications(GetNotificationsInput input);
        Task<GetNotificationsCountOutput> GetStartUpNotifications(GetNotificationsInput input);
        Task<GetNotificationsCountOutput> GetNotificationsCount(GetNotificationsInput input);
        Task<UserNotification> GetLatestUserNotification(GetNotificationsInput input);
        Task UpdateAllUserNotificationStates();
        Task UpdateUserNotificationState(Guid userNotificationId);
        Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input);
        Task<DeleteSelectedOutput> DeleteSelected(DeleteSelectedInput input);
        Task DeleteUserNotification(DeleteUserNotificationInput input);
    }

}
