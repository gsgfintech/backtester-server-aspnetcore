using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core;
using System;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep1ViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; set; }

        public CreateJobStep1ViewModel()
        {
            Settings = new BacktestJobSettingsModel()
            {
                StartDate = DateTimeUtils.GetLastBusinessDayInHKT(),
                EndDate = DateTimeUtils.GetLastBusinessDayInHKT(),
                StartTime = new DateTime(1, 1, 1, 6, 15, 0),
                EndTime = new DateTime(1, 1, 1, 16, 30, 0),
                UseHistoDatabase = true
            };
        }

        public CreateJobStep1ViewModel(BacktestJobSettingsModel settings)
        {
            Settings = settings;
        }
    }
}
