using Abp.BackgroundJobs;
using Abp.Dependency;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Transactions.FuelTransactions;
using System.Globalization;

namespace Sayarah.Application.Helpers.BackgroundJobs
{
    class NotifyUsersAfterFuelTransactionScheduleJob : BackgroundJob<NotifyInputDto>, ITransientDependency
    {
       
        private readonly IFuelTransOutAppService _fuelTransOutAppService;
        public NotifyUsersAfterFuelTransactionScheduleJob(
            IFuelTransOutAppService fuelTransOutAppService
            )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _fuelTransOutAppService = fuelTransOutAppService;
        }

        public override async void Execute(NotifyInputDto args)
        {
            try
            {
                await _fuelTransOutAppService.NotifyUsers(args);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("NotifyUsersAfterFuelTransactionScheduleJob ::: error  {0} !", ex.Message));
            }
        }
    }


    [Serializable]
    public class NotifyUsersAfterFuelTransactionScheduleJobArgs
    {
        public long SubscriptionId { get; set; }
    }
      
}
