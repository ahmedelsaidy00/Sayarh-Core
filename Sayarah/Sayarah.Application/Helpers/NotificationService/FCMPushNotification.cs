using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers.NotificationService
{
    public class FCMPushNotification
    {
        private static FirebaseApp _firebaseApp;
        static FCMPushNotification()
        {
            InitializeFirebase();
        }
        private static void InitializeFirebase()
        {
            if (_firebaseApp == null)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebaseAdminsdk.json");
                try
                {
                    // Initialize Firebase Admin SDK
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(path),
                    });
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    throw new InvalidOperationException("Failed to initialize Firebase SDK", ex);
                }
            }
        }
        public bool Successful { get; set; }
        public string Response { get; set; }
        public Exception Error { get; set; }



        public async Task<FCMPushNotification> SendNotification(FcmNotificationInput input)
        {
            FCMPushNotification result = new FCMPushNotification();
            try
            {
                //input.OrderId = input.OrderId ?? 0;
                //input.PatternId = input.PatternId ?? 0;
                //var serializer = new JavaScriptSerializer();

                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Token = input.RegistrationToken,
                    Notification = new Notification()
                    {
                        Title = input.Title,
                        Body = input.Body,
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "title", input.Title.ToString() },
                        { "body", input.Body.ToString() },
                        { "type", ((int)input.Type).ToString() },
                        { "patternId", input.PatternId?.ToString() },
                        { "userId", input.UserId?.ToString() },
                        { "unReadCount", input.UnReadCount.ToString() },
                        { "sound", "notification.mp3" },
                        { "android_channel_id", "CH_ID" },
                        { "priority", "high" }
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Sound = "notification.mp3",
                            ChannelId = "high_importance_channel"
                        }

                    },
                    Apns = new ApnsConfig()
                    {
                        Headers = new Dictionary<string, string>()
                         {
                             {"apns-priority", "10" }
                         },
                        Aps = new Aps()
                        {
                            Sound = "notification.mp3",
                            ContentAvailable = true

                        }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                result.Successful = true;
                result.Response = response;
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Response = null;
                result.Error = ex;
            }

            return result;
        }

        public async Task<FCMPushNotification> SendNotificationToAllDevices(FcmNotificationInput input)
        {
            FCMPushNotification result = new FCMPushNotification();
            try
            {
                //var serializer = new JavaScriptSerializer();
                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Topic = "alldevices",
                    Notification = new Notification()
                    {
                        Title = input.Title,
                        Body = input.Body,
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "title", input.Title },
                        { "body", input.Body },
                        { "type", ((int)input.Type).ToString() },
                        { "patternId", input.PatternId?.ToString() },
                        { "userId", input.UserId?.ToString() },
                        { "sound", "notification.mp3" },
                        { "android_channel_id", "CH_ID" },
                        { "priority", "high" }
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    }

                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                result.Successful = true;
                result.Response = response;
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Response = null;
                result.Error = ex;
            }

            return result;
        }

    }

    public enum FcmApplicationEnum
    {
        Client = 1
    }
    public class FcmApplication
    {
        public FcmApplication(FcmApplicationEnum fcmApplicationEnum)
        {
            switch (fcmApplicationEnum)
            {
                case FcmApplicationEnum.Client:
                    Key = "";
                    SenderId = "";

                    //Key = "";
                    //SenderId = "";

                    break;
                default:
                    break;
            }
        }
        public string Key { get; set; }
        public string SenderId { get; set; }
    }

    public enum FcmNotificationType
    {
        Public = 1,
        Logout = 2,
        QuestionReply = 3,
        Deactive = 4,
        FuelTransaction = 5

    }

    public enum FcmNotificationUserType
    {
        Client = 1
    }

    public class FcmNotificationInput
    {
        public string RegistrationToken { get; set; }
        public string Title { get; set; }
        public string SenderUserName { get; set; }
        public FcmNotificationType Type { get; set; }
        public long? PatternId { get; set; }
        public long? UserId { get; set; }
        public string Body { get; set; }
        public string Message { get; set; }
        public int UnReadCount { get; set; }

        public long? BranchId { get; set; }
        public long? CompanyId { get; set; }
        public long? ProviderId { get; set; }
        public long? MainProviderId { get; set; }
        public long? DriverId { get; set; }
        public long? WorkerId { get; set; }
        public decimal Price { get; set; }

    }
}
