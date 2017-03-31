using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.Trades
{
    [Route("api/tradegenericmetric2s")]
    public class TradeGenericMetric2SeriesApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradeGenericMetric2SeriesApiController>();

        private readonly TradeGenericMetric2SeriesControllerUtils utils;

        public TradeGenericMetric2SeriesApiController(TradeGenericMetric2SeriesControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost]
        public async Task<(bool, string)> Post([FromBody]BacktestTradeGenericMetric2Serie tradeGenericMetric2Serie)
        {
            return await utils.HandleNewTradeGenericMetric2Serie(tradeGenericMetric2Serie);
        }
    }
}
