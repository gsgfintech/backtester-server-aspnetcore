using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.ControllerUtils
{
    public class TradesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradesControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        public TradesControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        internal GenericActionResult HandleNewTrade(string backtestJobName, BacktestTrade trade)
        {
            if (trade == null)
                return new GenericActionResult(false, "Invalid trade object: null");

            logger.Debug($"Processing new trade {trade.TradeId}");

            return jobsControllerUtils.AddTrade(backtestJobName, trade);
        }
    }
}
