using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.ControllerUtils
{
    public class OrdersControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<OrdersControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        public OrdersControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        internal GenericActionResult HandleOrderUpdate(string backtestJobName, BacktestOrder order)
        {
            if (order == null)
                return new GenericActionResult(false, "Invalid order object: null");

            logger.Debug($"Processing order update {order.OrderId}");

            return jobsControllerUtils.AddOrder(backtestJobName, order);
        }
    }
}
