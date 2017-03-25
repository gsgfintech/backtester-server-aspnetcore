using Backtester.Server.Models;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep2ViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; set; }

        public CreateJobStep2ViewModel() { }

        public CreateJobStep2ViewModel(BacktestJobSettingsModel settings)
        {
            Settings = settings;
        }
    }
}
