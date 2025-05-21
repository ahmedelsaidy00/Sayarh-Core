using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using System;
using System.Globalization;


namespace Sayarah.Application.Helpers
{
    class SendReminderScheduleJob : BackgroundJob<SendReminderScheduleJobArgs>, ITransientDependency
    {
        //private readonly IFollowupAppService _followupService;

        //CultureInfo new_lang = new CultureInfo("ar");
        //public SendReminderScheduleJob(IFollowupAppService followupService)
        //{
        //    LocalizationSourceName = RamSystemConsts.LocalizationSourceName;
        //    _followupService = followupService;
        //}
         
      
        public override void Execute(SendReminderScheduleJobArgs args)
        {
            try
            {
             
           // _followupService.NotifyUser(new NotifyUserInput{UserId = args.UserId, ClientId = args.ClientId});
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("SendReservationSchedulecJob ::: error  {0} !", ex.Message));
            }
        }


        ////////////////////////////////////Helper///////////////////////////////////
        private string GetTimeFromNow(TimeSpan RemainingTime, CultureInfo new_lang)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = RemainingTime;
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? L("Pages.NotificationsTime.OneSecond", new_lang) : L("Pages.NotificationsTime.Seconds", new_lang, ts.Seconds);

            if (delta < 2 * MINUTE)
                return L("Pages.NotificationsTime.OneMinute", new_lang);

            if (delta < 45 * MINUTE)
                return L("Pages.NotificationsTime.Minutes", new_lang, ts.Minutes);

            if (delta < 90 * MINUTE)
                return L("Pages.NotificationsTime.OneHour", new_lang);

            if (delta < 24 * HOUR)
                return L("Pages.NotificationsTime.Hours", new_lang, ts.Hours) +" "+ L("Pages.NotificationsTime.AndMinutes", new_lang, ts.Minutes);

            if (delta < 48 * HOUR)
                return L("Pages.NotificationsTime.Yesterday", new_lang);

            if (delta < 30 * DAY)
                return L("Pages.NotificationsTime.Days", new_lang, ts.Days);

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? L("Pages.NotificationsTime.OneMonth", new_lang) : L("Pages.NotificationsTime.Months", new_lang, months);
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? L("Pages.NotificationsTime.OneYear", new_lang) : L("Pages.NotificationsTime.Years", new_lang, years);
            }

        }
    }



    [Serializable]
    public class SendReminderScheduleJobArgs
    {
        public long ClientId { get; set; }
        public long UserId { get; set; }
    }
}
