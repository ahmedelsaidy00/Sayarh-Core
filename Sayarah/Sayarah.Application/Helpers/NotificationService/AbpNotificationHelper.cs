using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Notifications;
using Abp;
using System;
using System.Linq;
using static Sayarah.SayarahConsts;
using Sayarah.Contact;
using Abp.Auditing;
using Sayarah.Application;
using Sayarah.Core.Helpers;

namespace Sayarah.Application.Helpers.NotificationService
{
    [AbpAuthorize]
    public class AbpNotificationHelper : SayarahAppServiceBase, IAbpNotificationHelper
    {
        //private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly INotificationPublisher _notificationPublisher;
        //private readonly INotificationStore _notificationStore;
        private readonly IUserNotificationManager _userNotificationManager;
        public AbpNotificationHelper(
            //INotificationSubscriptionManager notificationSubscriptionManager,
            INotificationPublisher notificationPublisher,
            //INotificationStore notificationStore,
            IUserNotificationManager userNotificationManager)
        {
            // _notificationSubscriptionManager = notificationSubscriptionManager;
            _notificationPublisher = notificationPublisher;
            // _notificationStore = notificationStore;
            _userNotificationManager = userNotificationManager;
        }

        #region private For AbpNotifications

        //Send a general notification to a specific user
        public async Task Publish_CreateNotification(string notificationName, CreateNotificationDto CreateNotificationData, UserIdentifier[] targetUsersId)
        {
            await _notificationPublisher.PublishAsync(notificationName, CreateNotificationData, severity: NotificationSeverity.Success, userIds: targetUsersId);
        }

        //GetUserAbpNotifications
        //GetUserAbpNotifications
        public async Task<GetNotificationsOutput> GetUserAbpNotifications(GetNotificationsInput input)
        {
            var notifications = new List<UserNotification>();
            int notificationsCount = 0;
            if (input.State.HasValue)
            {
                UserNotificationState notificationState = (UserNotificationState)input.State;

                if (input.EntityType.HasValue)
                {
                    notifications = await _userNotificationManager
                    .GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), notificationState);

                    var notificationName = string.Empty;
                    switch (input.EntityType)
                    {
                        case Entity_Type.ContactMsg:
                            notificationName = NotificationsNames.NewContactMsg;
                            break;

                        case Entity_Type.NewClientRegisteration:
                            notificationName = NotificationsNames.NewRegisteration;
                            break;

                        default:
                            break;
                    }

                    notifications = notifications.Where(x => x.Notification.NotificationName == notificationName).ToList();

                    notificationsCount = notifications.Count();
                    notifications = notifications.OrderByDescending(x => x.Notification.CreationTime).Skip(input.Start.Value).Take(input.Length.Value).ToList();
                }
                else
                {
                    notificationsCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), notificationState);
                    notifications = await _userNotificationManager
                        .GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), notificationState, input.Start.Value, input.Length.Value);
                }
            }
            else
            {
                if (input.EntityType.HasValue)
                {
                    notifications = await _userNotificationManager
                            .GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), null);
                    var notificationName = string.Empty;
                    switch (input.EntityType)
                    {
                        case Entity_Type.ContactMsg:
                            notificationName = NotificationsNames.NewContactMsg;
                            break;
                        case Entity_Type.NewClientRegisteration:
                            notificationName = NotificationsNames.NewRegisteration;
                            break;
                        default:
                            break;
                    }

                    notifications = notifications.Where(x => x.Notification.NotificationName == notificationName).ToList();
                    notificationsCount = notifications.Count();
                    notifications = notifications.OrderByDescending(x => x.Notification.CreationTime).Skip(input.Start.Value).Take(input.Length.Value).ToList();
                }
                else
                {
                    notificationsCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId));
                    notifications = await _userNotificationManager
                          .GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), null, input.Start.Value, input.Length.Value);
                }
            }

            return new GetNotificationsOutput { Notifications = notifications, TotalCount = notificationsCount };
        }

        //getStartUpNotifications

        [DisableAuditing]
        public async Task<GetNotificationsCountOutput> GetStartUpNotifications(GetNotificationsInput input)
        {
            int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), UserNotificationState.Unread);
            int totalCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId));
            var notifications = await _userNotificationManager.GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), UserNotificationState.Unread);
            var latestNotification = notifications.FirstOrDefault();
            return new GetNotificationsCountOutput { UnReadCount = unReadCount, TotalCount = totalCount, Notification = latestNotification };
        }

        //getUnReadNotifications
        public async Task<GetNotificationsCountOutput> GetNotificationsCount(GetNotificationsInput input)
        {
            int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), UserNotificationState.Unread);
            int totalCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId));

            return new GetNotificationsCountOutput { UnReadCount = unReadCount, TotalCount = totalCount };
        }

        //getLatestUserNotification
        public async Task<UserNotification> GetLatestUserNotification(GetNotificationsInput input)
        {
            var notifications = await _userNotificationManager.GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), UserNotificationState.Unread);
            var latestNotification = notifications.FirstOrDefault();

            return latestNotification;
        }

        //UpdateAllUserNotificationStates
        public async Task UpdateAllUserNotificationStates()
        {
            await _userNotificationManager
                .UpdateAllUserNotificationStatesAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: AbpSession.UserId.Value), UserNotificationState.Read);
        }

        //UpdateUserNotificationState
        public async Task UpdateUserNotificationState(Guid userNotificationId)
        {
            await _userNotificationManager
                .UpdateUserNotificationStateAsync(AbpSession.TenantId, userNotificationId, UserNotificationState.Read);
        }


        //DeleteAllUserNotifications
        public async Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input)
        {
            try
            {


                if (input.EntityType.HasValue && input.EntityType.Value > 0)
                {
                    var notifications = await _userNotificationManager
                                  .GetUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), null);
                    var notificationName = string.Empty;
                    switch (input.EntityType)
                    {
                        case Entity_Type.ContactMsg:
                            notificationName = NotificationsNames.NewContactMsg;
                            break;
                        case Entity_Type.NewClientRegisteration:
                            notificationName = NotificationsNames.NewRegisteration;
                            break;
                        default:
                            break;
                    }
                    notifications = notifications.Where(x => x.Notification.NotificationName == notificationName).ToList();
                    if (notifications != null && notifications.Count > 0)
                    {
                        foreach (var item in notifications)
                        {
                            await DeleteUserNotification(new DeleteUserNotificationInput { UserNotificationId = item.Id });
                        }

                    }

                }
                else
                {
                    await _userNotificationManager.DeleteAllUserNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId));

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        //DeleteSelectedNotifications
        public async Task<DeleteSelectedOutput> DeleteSelected(DeleteSelectedInput input)
        {
            try
            {
                if (input.NotificationIds != null && input.NotificationIds.Count > 0)
                {
                    foreach (var itemId in input.NotificationIds)
                    {
                        await DeleteUserNotification(new DeleteUserNotificationInput { UserNotificationId = itemId });
                    }

                }
                int unReadCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId), UserNotificationState.Unread);
                // int totalCount = await _userNotificationManager.GetUserNotificationCountAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.UserId));
                return new DeleteSelectedOutput { UnReadCount = unReadCount };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //DeleteUserNotification
        public async Task DeleteUserNotification(DeleteUserNotificationInput input)
        {
            await _userNotificationManager
                .DeleteUserNotificationAsync(AbpSession.TenantId, input.UserNotificationId);
        }


        //Subscribe to a general notification
        //public async Task Subscribe_CreateOrderNotification(int? tenantId, long userId)
        //    {
        //        await _notificationSubscriptionManager.SubscribeAsync(new UserIdentifier(tenantId, userId), SayarahConsts.NotificationsNames.CreateOrderNotification);
        //    }

        //using  INotificationStore
        //public async Task<List<UserNotificationInfoWithNotificationInfo>> Subscribe_GetUserOrderNotifications()
        //{
        //    var notys = await _notificationStore.GetUserNotificationsWithNotificationsAsync(new UserIdentifier(tenantId: AbpSession.TenantId, userId: AbpSession.UserId.Value), UserNotificationState.Unread, 0, int.MaxValue);
        //    return notys;
        //}

        ////Subscribe to an entity notification
        //public async Task Subscribe_CommentPhoto(int? tenantId, long userId, Guid orderId)
        //{
        //    await _notificationSubscriptionManager.SubscribeAsync(new UserIdentifier(tenantId, userId), "CommentPhoto", new EntityIdentifier(typeof(Order), orderId));
        //}
        #endregion
    }




    [Serializable]
    public class CreateNotificationDto : NotificationData
    {
        public long? SenderUserId { get; set; }
        public string SenderUserName { get; set; }
        public string Message { get; set; }
        public Entity_Type EntityType { get; set; }
        public long? EntityId { get; set; }
        public string EntityName { get; set; }
        public UserTypes? UserType { get; set; }
        public ContactsType ContactType { get; set; }
        public long? SubscriptionId { get; set; }

        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? WorkerId { get; set; }

        public TicketFrom TicketFrom { get; set; }


        //public decimal Price { get; set; }
        //public long? OfferId { get; set; }
        //public string AdvertisementName { get; set; }
        //public string AdvertisementCode { get; set; }
    }
    public enum Entity_Type
    {
        ContactMsg = 1,
        NewClientRegisteration = 2,
        Public = 3,
        EmploymentMsg = 4,  
        BrancheRequest = 5,
        RejectBrancheRequest = 6,
        NewRequestOrder = 7,
        NewSubscribePackage = 8,
        Logout = 9,
        AcceptFuelPriceChangeRequest = 10,
        RefuseFuelPriceChangeRequest = 11,
        NewFuelPriceChangeRequest = 12,
        VeichleUpdated = 13,
        FuelTransaction = 14,
        Wallet = 15,
        NewInvoice = 16,
        NewVoucher = 17,
        NewWalletTransfer = 18,
        AcceptWalletTransfer = 19,
        RefuseWalletTransfer = 20,
        AcceptSubscriptionTransfer = 21,
        RefuseSubscriptionTransfer = 22,
        ExpireSubscription = 23,
        NewRegisterationRequest = 24,
        SubscribePackageWalletError = 25,
        UpgradeSubscription = 26,
        NewTicket = 27,
        TicketCompleted = 28

    }
    public class GetNotificationsInput
    {
        public long UserId { get; set; }
        public Entity_Type? EntityType { get; set; }
        public int? State { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
    }
    public class GetNotificationsOutput
    {
        public List<UserNotification> Notifications { get; set; }
        public int TotalCount { get; set; }
    }

    public class GetNotificationsCountOutput
    {
        public int TotalCount { get; set; }
        public int UnReadCount { get; set; }
        public UserNotification Notification { get; set; }

    }
    public class DeleteSelectedInput
    {
        public long UserId { get; set; }
        public List<Guid> NotificationIds { get; set; }
    }
    public class DeleteSelectedOutput
    {
        public int UnReadCount { get; set; }
    }
    public class DeleteAllUserNotificationsInput
    {
        public Entity_Type? EntityType { get; set; }
        public long UserId { get; set; }
    }

    public class DeleteUserNotificationInput
    {
        public Guid UserNotificationId { get; set; }
    }

    public class NotificationGuidObject
    {
        public Guid UserNotificationId { get; set; }
    }
    public class NotificationsCountOutput
    {
        public int TotalCount { get; set; }
        public int UnReadCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class MobileUserNotificationOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }
        public List<MobileUserNotification> Notifications { get; set; }
        public int UnReadCount { get; set; }
    }
    public class MobileUserNotification
    {
        public Guid Id { get; set; }
        public UserNotificationState State { get; set; }
        public DateTime CreationTime { get; set; }
        public NotificationData Data { get; set; }
    }
    public class DeleteOrderNotificationsInput
    {
        public long OrderId { get; set; }
    }
}