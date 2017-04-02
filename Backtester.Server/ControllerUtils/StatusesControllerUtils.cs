using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.ControllerUtils
{
    public class StatusesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<StatusesControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        public StatusesControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        internal GenericActionResult HandleStatusUpdate(string backtestJobName, BacktestJobStatus status)
        {
            if (status == null)
                return new GenericActionResult(false, "Invalid status object: null");

            logger.Debug($"Processing status update for {backtestJobName}");

            return jobsControllerUtils.AddStatusUpdate(backtestJobName, status);
        }
    }
}
