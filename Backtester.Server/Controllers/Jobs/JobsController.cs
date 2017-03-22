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

        public IActionResult Orders(string jobId)
        {
            return ViewComponent("JobOrders", jobId);
        }

        public IActionResult Trades(string jobId)
        {
            return ViewComponent("JobTrades", jobId);
        }

        public IActionResult Positions(string jobId)
        {
            return ViewComponent("JobPositions", jobId);
        }
    }
}
