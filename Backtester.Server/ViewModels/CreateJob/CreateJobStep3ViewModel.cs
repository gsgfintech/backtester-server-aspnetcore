using Backtester.Server.Models;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep3ViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; set; }

        public CreateJobStep3ViewModel() { }

        public CreateJobStep3ViewModel(BacktestJobSettingsModel settings)
        {
            Settings = settings;
        }
    }
}
