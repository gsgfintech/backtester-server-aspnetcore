namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobSubmitViewModel
    {
        public string JobName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public CreateJobSubmitViewModel() { }

        public CreateJobSubmitViewModel(string jobName, bool success, string message)
        {
            JobName = jobName;
            Success = success;
            Message = message;
        }
    }
}
