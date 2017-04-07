using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels.Jobs;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Status(string jobGroupId, string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobStatusViewModel(job.ToBacktestJobModel()));
        }

        public async Task<IActionResult> Alerts(string jobGroupId, string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobAlertsViewModel(jobGroupId, jobId, job?.Output?.Alerts));
        }

        public async Task<IActionResult> Orders(string jobGroupId, string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobOrdersViewModel(jobGroupId, jobId, job?.Output?.Orders?.Values));
        }

        public async Task<IActionResult> Trades(string jobGroupId, string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobTradesViewModel(jobGroupId, jobId, job?.Output?.Trades?.Values));
        }

        public async Task<IActionResult> Positions(string jobGroupId, string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobPositionsViewModel(jobGroupId, jobId, job?.Output?.Positions));
        }
    }
}
