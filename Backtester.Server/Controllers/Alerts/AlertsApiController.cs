using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Alerts
{
    [Route("api/alerts")]
    public class AlertsApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<AlertsApiController>();

        private readonly AlertsControllerUtils utils;

        public AlertsApiController(AlertsControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("{backtestJobName}")]
        public GenericActionResult Post(string backtestJobName, [FromBody]Alert alert)
        {
            return utils.HandleNewAlert(backtestJobName, alert);
        }
    }
}
