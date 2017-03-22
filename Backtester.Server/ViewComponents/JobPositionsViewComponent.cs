using Backtester.Server.ControllerUtils;
using Backtester.Server.ViewModels;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobPositionsViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobPositionsViewComponent>();

        private readonly JobsControllerUtils utils;

        public JobPositionsViewComponent(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobPositionsViewModel(job?.Output?.Positions));
        }
    }
}
