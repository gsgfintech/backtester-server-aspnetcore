using Backtester.Server.ControllerUtils;
using Backtester.Server.ViewModels;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class AllTradesViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobViewComponent>();

        private readonly JobGroupsControllerUtils utils;

        public AllTradesViewComponent(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string groupId)
        {
            var trades = await utils.GetTrades(groupId);

            return View(new AllTradesViewModel() { GroupId = groupId, Trades = trades });
        }
    }
}
