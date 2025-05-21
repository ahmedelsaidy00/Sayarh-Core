using Abp.BackgroundJobs;
using Abp.Dependency;
using Sayarah.Application.Veichles;
using System.Globalization;

namespace Sayarah.Application.Helpers.BackgroundJobs
{
    class EndFuelGroupPeriodScheduleJob : BackgroundJob<EndFuelGroupPeriodScheduleJobArgs>, ITransientDependency
    {
       
        private readonly IVeichleAppService _veichleAppService;
        CultureInfo new_lang = new CultureInfo("ar");
        public EndFuelGroupPeriodScheduleJob(
            IVeichleAppService veichleAppService
            )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _veichleAppService = veichleAppService;
        }

        public override async void Execute(EndFuelGroupPeriodScheduleJobArgs args)
        {
            try
            {
                await _veichleAppService.HandleEndFuelGroupPeriod(args);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("EndFuelGroupPeriodScheduleJob ::: error  {0} !", ex.Message));
            }
        }
    }


    [Serializable]
    public class EndFuelGroupPeriodScheduleJobArgs
    {
        public long VeichleId { get; set; }
        public int PeriodScheduleCount { get; set; }
        public DateTime MoneyBalanceEndDate { get; set; }
    }
      
}
