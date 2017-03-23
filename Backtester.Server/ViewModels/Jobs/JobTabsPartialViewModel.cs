namespace Backtester.Server.ViewModels.Jobs
{
    public class JobTabsPartialViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }

        public JobTabsPartialViewModel(string jobGroupId, string jobId)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;
        }
    }
}
