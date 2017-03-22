using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobOrdersViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobOrdersViewComponent>();

        private readonly JobsControllerUtils utils;

        public JobOrdersViewComponent(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobId)
        {
            var job = await utils.Get(jobId);

            return View(new JobOrdersViewModel(job?.Output?.Orders?.Values));
        }
    }
}
