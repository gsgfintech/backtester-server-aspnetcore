using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Positions
{
    [Route("api/positions")]
    public class PositionsApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<PositionsApiController>();

        private readonly PositionsControllerUtils utils;

        public PositionsApiController(PositionsControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("{backtestJobName}")]
        public GenericActionResult Post(string backtestJobName, [FromBody]BacktestPosition position)
        {
            return utils.HandlePositionUpdate(backtestJobName, position);
        }
    }
}
