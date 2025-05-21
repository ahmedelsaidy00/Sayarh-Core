using Abp.BackgroundJobs;
using Abp.Dependency;
using Sayarah.Application.Subscriptions;
using System;
using System.Globalization;

namespace Sayarah.Application.Helpers.BackgroundJobs
{
    class AfterSubscriptionEndDateScheduleJob : BackgroundJob<AfterSubscriptionEndDateScheduleJobArgs>, ITransientDependency
    {
       
        private readonly ISubscriptionAppService _subscriptionAppService;
        CultureInfo new_lang = new CultureInfo("ar");
        public AfterSubscriptionEndDateScheduleJob(
            ISubscriptionAppService subscriptionAppService
            )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _subscriptionAppService = subscriptionAppService;
        }

        public override async void Execute(AfterSubscriptionEndDateScheduleJobArgs args)
        {
            try
            {
                await _subscriptionAppService.HandleAfterSubscriptionEndDate(new Abp.Application.Services.Dto.EntityDto<long> { Id = args.SubscriptionId });
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("AfterSubscriptionEndDateScheduleJob ::: error  {0} !", ex.Message));
            }
        }
    }


    [Serializable]
    public class AfterSubscriptionEndDateScheduleJobArgs
    {
        public long SubscriptionId { get; set; }
    }
      
}
