using System.Collections.Generic;

namespace Backtester.Server.ViewModels
{
    public class JobGroupTabsViewModel
    {
        public string JobGroupId { get; private set; }

        public Dictionary<string, string> JobIds { get; private set; }

        public JobGroupTabsViewModel(string jobGroupId, Dictionary<string, string> jobIds)
        {
            JobGroupId = jobGroupId;
            JobIds = jobIds ?? new Dictionary<string, string>();
        }
    }
}
