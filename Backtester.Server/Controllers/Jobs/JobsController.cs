using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Jobs
{
    [Authorize]
    public class JobsController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsController>();

        private readonly JobsControllerUtils utils;

        public JobsController(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public IActionResult Index(string jobId)
        {
            return ViewComponent("Job", jobId);
        }

        public IActionResult Status(string jobId)
        {
            return ViewComponent("JobStatus", jobId);
        }
    }
}
