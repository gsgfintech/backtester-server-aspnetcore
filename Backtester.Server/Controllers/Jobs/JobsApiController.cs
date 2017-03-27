using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.Jobs
{
    [Route("api/jobs")]
    public class JobsApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsApiController>();

        private readonly JobsControllerUtils utils;

        public JobsApiController(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("update-status/{backtestJobName}")]
        public async Task<GenericActionResult> UpdateStatus(string backtestJobName, [FromBody]BacktestJob job)
        {
            return await utils.UpdateJob(backtestJobName, job);
        }
    }
}
