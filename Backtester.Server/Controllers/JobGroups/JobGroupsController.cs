using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backtester.Server.ControllerUtils;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Utils.Core.Logging;
using Backtester.Server.Models;

namespace Backtester.Server.Controllers.JobGroups
{
    public class JobGroupsController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsController>();

        private readonly JobGroupsControllerUtils utils;

        public JobGroupsController(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(string groupId)
        {
            return View(await LoadJobGroup(groupId));
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
