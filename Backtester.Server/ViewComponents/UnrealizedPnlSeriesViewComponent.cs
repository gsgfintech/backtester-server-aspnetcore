using Backtester.Server.ControllerUtils;
using Backtester.Server.ViewModels;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class UnrealizedPnlSeriesViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesViewComponent>();

        private readonly UnrealizedPnlSeriesControllerUtils utils;

        public UnrealizedPnlSeriesViewComponent(UnrealizedPnlSeriesControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(string jobGroupId)
        {
            var unrealizedPnlSeries = await utils.GetForJobGroup(jobGroupId);

            return View(new UnrealizedPnlSeriesViewModel(unrealizedPnlSeries));
        }
    }
}
