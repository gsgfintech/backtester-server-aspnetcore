using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep4ViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; set; }

        [Display(Name = "Total Business Days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalDays => DateTimeUtils.EachBusinessDay(Settings.StartDate, Settings.EndDate).Count();

        public CreateJobStep4ViewModel() { }

        public CreateJobStep4ViewModel(BacktestJobSettingsModel settings)
        {
            Settings = settings;
        }
    }
}
