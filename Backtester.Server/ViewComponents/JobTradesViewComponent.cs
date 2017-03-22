using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobTradesViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobTradesViewComponent>();

        private readonly JobsControllerUtils utils;

        public JobTradesViewComponent(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobId)
        {
            var job = await utils.Get(jobId);

            return View(job?.Output?.Trades?.Values?.ToTradeModels());
        }
    }
}
