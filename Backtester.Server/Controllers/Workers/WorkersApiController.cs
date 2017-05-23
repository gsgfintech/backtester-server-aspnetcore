using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Utils.Core.Logging;
using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Data.Core.SystemData;
using System.Threading.Tasks;

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
        public GenericActionResult UpdateStatus(string workerName, [FromBody]SystemStatus status)
        {
            return utils.HandleStatusUpdate(workerName, status);
        }

        [HttpGet("{workerName}/request-job")]
        public GenericActionResult<string> RequestJob(string workerName)
        {
            return utils.RequestJob(workerName);
        }
         
        [HttpGet("acceptjobs/{workerName}")]
        public async Task<GenericActionResult> AcceptJobs(string workerName)
        {
            var result = await utils.AcceptJobs(workerName);

            return new GenericActionResult(result.Success, result.Message);
        }

        [HttpGet("rejectjobs/{workerName}")]
        public async Task<GenericActionResult> RejectJobs(string workerName)
        {
            var result = await utils.RejectJobs(workerName);

            return new GenericActionResult(result.Success, result.Message);
        }

        [HttpGet("start/{workerName}")]
        public async Task<GenericActionResult> Start(string workerName)
        {
            var result = await utils.Start(workerName);

            return new GenericActionResult(result.Success, result.Message);
        }

        [HttpGet("stop/{workerName}")]
        public async Task<GenericActionResult> Stop(string workerName)
        {
            var result = await utils.Stop(workerName);

            return new GenericActionResult(result.Success, result.Message);
        }
    }
}
