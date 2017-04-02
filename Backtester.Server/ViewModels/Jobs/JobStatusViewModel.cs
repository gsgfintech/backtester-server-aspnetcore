using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobStatusViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }
        public BacktestJobStatusModel Status { get; private set; }

        public JobStatusViewModel(string jobGroupId, string jobId, BacktestJobStatus status)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;
            Status = status.ToBacktestStatusModel();
        }
    }
}
