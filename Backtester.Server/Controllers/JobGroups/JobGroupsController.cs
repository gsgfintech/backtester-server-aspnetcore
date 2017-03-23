using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels;
using Backtester.Server.ViewModels.JobGroups;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.JobGroups
{
    [Authorize]
    public class JobGroupsController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsController>();

        private readonly JobGroupsControllerUtils utils;
        private readonly UnrealizedPnlSeriesControllerUtils unrealizedPnlSeriesControllerUtils;

        public JobGroupsController(JobGroupsControllerUtils utils, UnrealizedPnlSeriesControllerUtils unrealizedPnlSeriesControllerUtils)
        {
            this.utils = utils;
            this.unrealizedPnlSeriesControllerUtils = unrealizedPnlSeriesControllerUtils;
        }

        public async Task<IActionResult> Index(string groupId)
        {
            return View(await LoadJobGroup(groupId));
        }

        public async Task<IActionResult> Info(string groupId)
        {
            var group = await utils.Get(groupId);

            return View(new JobGroupInfoViewModel(group));
        }

        public async Task<IActionResult> AllTrades(string groupId)
        {
            var group = await utils.Get(groupId);

            return View(new JobGroupAllTradesViewModel(groupId, group?.Trades));
        }

        public async Task<IActionResult> UnrealizedPnls(string groupId)
        {
            var unrealizedPnlSeries = await unrealizedPnlSeriesControllerUtils.GetForJobGroup(groupId);

            return View(new UnrealizedPnlSeriesViewModel(groupId, unrealizedPnlSeries));
        }

        //public IActionResult Create()
        //{
        //    return View();
        //}

        private async Task<BacktestJobGroupModel> LoadJobGroup(string groupId)
        {
            return (await utils.Get(groupId)).ToBacktestJobGroupModel();
        }
    }
}
