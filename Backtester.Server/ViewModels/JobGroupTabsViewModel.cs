using Backtester.Server.Models;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels
{
    public class JobGroupTabsViewModel
    {
        public string JobGroupId { get; private set; }

        public Dictionary<string, BacktestJobLightModel> Jobs { get; private set; }

        public JobGroupTabsViewModel(string jobGroupId, Dictionary<string, BacktestJobLightModel> jobs)
        {
            JobGroupId = jobGroupId;
            Jobs = jobs ?? new Dictionary<string, BacktestJobLightModel>();
        }
    }
}
