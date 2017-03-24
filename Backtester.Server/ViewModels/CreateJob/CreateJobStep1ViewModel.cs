using Backtester.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels.CreateJob
{
    public class CreateJobStep1ViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public BacktestJobSettingsModel Settings { get; private set; }

        public CreateJobStep1ViewModel()
        {
            Settings = new BacktestJobSettingsModel();
        }
    }
}
