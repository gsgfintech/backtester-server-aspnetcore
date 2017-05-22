using Backtester.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels.CreateJobExcel
{
    public class CreateJobExcelReviewViewModel
    {
        public string Message { get; set; }

        public List<BacktestJobSettingsModel> JobsSettings { get; set; }

        public string JobNamesStr => string.Join(",", JobsSettings.Select(j => j.JobName));
    }
}
