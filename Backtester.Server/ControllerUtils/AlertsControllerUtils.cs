using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.ControllerUtils
{
    public class AlertsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<AlertsControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        public AlertsControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        internal GenericActionResult HandleNewAlert(string backtestJobName, Alert alert)
        {
            if (alert == null)
                return new GenericActionResult(false, "Invalid alert object: null");

            logger.Debug($"Processing new alert {alert.AlertId}");

            return jobsControllerUtils.AddAlert(backtestJobName, alert);
        }
    }
}
