using Backtester.Server.Models;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Utils.Core;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobAlertsViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }
        public List<AlertModel> Alerts { get; private set; }

        public JobAlertsViewModel(string jobGroupId, string jobId, IEnumerable<Alert> alerts)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;
            Alerts = !alerts.IsNullOrEmpty() ? alerts.OrderByDescending(a => a.Timestamp).ToAlertModels() : new List<AlertModel>();
        }
    }
}
