using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Utils.Core.Logging;
using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Data.Core.SystemData;

namespace Backtester.Server.Controllers.Workers
{
    [Route("api/workers")]
    public class WorkersApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<WorkersApiController>();

        private readonly WorkersControllerUtils utils;

        public WorkersApiController(WorkersControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("status/{workerName}")]
        public async Task<GenericActionResult> UpdateStatus(string workerName, [FromBody]SystemStatus status)
        {
            return await utils.HandleStatusUpdate(workerName, status);
        }

        [HttpGet("{workerName}/request-job")]
        public GenericActionResult<string> RequestJob(string workerName)
        {
            return utils.RequestJob(workerName);
        }
    }
}
