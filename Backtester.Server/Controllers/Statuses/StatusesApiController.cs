using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Statuses
{
    [Route("api/statuses")]
    public class StatusesApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<StatusesApiController>();

        private readonly StatusesControllerUtils utils;

        public StatusesApiController(StatusesControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("{backtestJobName}")]
        public GenericActionResult Post(string backtestJobName, [FromBody]BacktestJobStatus status)
        {
            return utils.HandleStatusUpdate(backtestJobName, status);
        }
    }
}
