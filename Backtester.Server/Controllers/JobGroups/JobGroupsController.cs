using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
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

        public JobGroupsController(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IActionResult> Index(string groupId)
        {
            return View(await LoadJobGroup(groupId));
        }

        public async Task<IActionResult> Info(string groupId)
        {
            return PartialView("JobGroupInfoPartial", await LoadJobGroup(groupId));
        }

        public async Task<IActionResult> AllTrades(string groupId)
        {
            return View(await LoadJobGroup(groupId));
        }

        public async Task<IActionResult> UnrealizedPnls(string groupId)
        {
            return View(await LoadJobGroup(groupId));
        }

        public IActionResult Create()
        {
            return View();
        }

        private async Task<BacktestJobGroupModel> LoadJobGroup(string groupId)
        {
            return (await utils.Get(groupId)).ToBacktestJobGroupModel();
        }
    }
}
