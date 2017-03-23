using Backtester.Server.Models;
using DataTypes.Core;

namespace Backtester.Server.ViewModels.JobGroups
{
    public class JobGroupInfoViewModel
    {
        public BacktestJobGroupModel JobGroup { get; private set; }

        public JobGroupInfoViewModel(BacktestJobGroup jobGroup)
        {
            JobGroup = jobGroup.ToBacktestJobGroupModel();
        }
    }
}
