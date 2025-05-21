using System;

namespace Sayarah.Application.Helpers.HangFire
{
    public class SampleBackgroundJob
    {
        public void ExecuteTask()
        {
            Console.WriteLine($"Task executed at: {DateTime.Now}");
        }
    }

}
