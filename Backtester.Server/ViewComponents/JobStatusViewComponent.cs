using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobStatusViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobStatusViewComponent>();

        private readonly JobsControllerUtils utils;

        public JobStatusViewComponent(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobId)
        {
            var job = await utils.Get(jobId);

            return View(job?.Output?.Status.ToBacktestStatusModel());
        }
    }
}
