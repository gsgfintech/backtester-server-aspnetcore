using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.UnrealizedPnlSeries
{
    [Route("api/unrpnls")]
    public class UnrealizedPnlSeriesApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesApiController>();

        private readonly UnrealizedPnlSeriesControllerUtils utils;

        public UnrealizedPnlSeriesApiController(UnrealizedPnlSeriesControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost]
        public GenericActionResult Post([FromBody]BacktestUnrealizedPnlSerie pnlSerie)
        {
            return utils.HandleNewUnrealizedPnlSerie(pnlSerie);
        }
    }
}
