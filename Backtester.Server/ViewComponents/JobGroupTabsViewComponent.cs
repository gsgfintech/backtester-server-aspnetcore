using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobGroupTabsViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupTabsViewComponent>();

        private readonly JobGroupsControllerUtils utils;

        public JobGroupTabsViewComponent(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobGroupId)
        {
            var group = await utils.Get(jobGroupId);

            return View(new JobGroupTabsViewModel(jobGroupId, group?.Jobs.ToBacktestJobLightModels()));
        }
    }
}
