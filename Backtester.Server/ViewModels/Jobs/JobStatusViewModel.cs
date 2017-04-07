using Backtester.Server.Models;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobStatusViewModel
    {
        public BacktestJobModel Job { get; set; }

        public JobStatusViewModel(BacktestJobModel job)
        {
            Job = job;
        }
    }
}
