using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.ControllerUtils
{
    public class PositionsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<PositionsControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        public PositionsControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        internal GenericActionResult HandlePositionUpdate(string backtestJobName, BacktestPosition position)
        {
            if (position == null)
                return new GenericActionResult(false, "Invalid position object: null");

            logger.Debug($"Processing position update");

            return jobsControllerUtils.AddPosition(backtestJobName, position);
        }
    }
}
