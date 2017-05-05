using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewComponents;
using Backtester.Server.ViewModels;
using Backtester.Server.ViewModels.JobGroups;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.JobGroups
{
    [Authorize]
    public class JobGroupsController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsController>();

        private readonly JobGroupsControllerUtils utils;
        private readonly JobsControllerUtils jobsControllerUtils;
        private readonly TradeGenericMetric2SeriesControllerUtils tradeGenericMetric2SeriesControllerUtils;
        private readonly UnrealizedPnlSeriesControllerUtils unrealizedPnlSeriesControllerUtils;

        public JobGroupsController(JobGroupsControllerUtils utils, UnrealizedPnlSeriesControllerUtils unrealizedPnlSeriesControllerUtils, TradeGenericMetric2SeriesControllerUtils tradeGenericMetric2SeriesControllerUtils, JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
            this.utils = utils;
            this.tradeGenericMetric2SeriesControllerUtils = tradeGenericMetric2SeriesControllerUtils;
            this.unrealizedPnlSeriesControllerUtils = unrealizedPnlSeriesControllerUtils;
        }

        public async Task<IActionResult> Index(string groupId)
        {
            return View(await LoadJobGroup(groupId));
        }

        public async Task<IActionResult> Info(string groupId)
        {
            var group = await utils.Get(groupId);

            var viewModel = new JobGroupInfoViewModel(group);

            foreach (var kvm in viewModel.JobGroup.Jobs)
            {
                var pnlResult = await jobsControllerUtils.GetNetRealizedPnl(kvm.Key);

                if (!pnlResult.Success)
                    logger.Error($"Failed to get net realized pnl for job {kvm.Key}: {pnlResult.Message}");

                viewModel.JobGroup.Jobs[kvm.Key].NetRealizedPnlUsd = pnlResult.NetRealizedPnlUsd;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> AllTrades(string groupId)
        {
            var group = await utils.Get(groupId);

            return View(new JobGroupAllTradesViewModel(groupId, group?.Trades ?? new List<BacktestTrade>()));
        }

        public async Task<IActionResult> UnrealizedPnls(string groupId)
        {
            var unrealizedPnlSeries = await unrealizedPnlSeriesControllerUtils.GetForJobGroup(groupId);

            return View(new UnrealizedPnlSeriesViewModel(groupId, unrealizedPnlSeries));
        }

        public async Task<IActionResult> Statistics(string groupId)
        {
            var statistics = await utils.ComputeStatistics(groupId);

            return View(statistics);
        }

        private async Task<BacktestJobGroupModel> LoadJobGroup(string groupId)
        {
            return (await utils.Get(groupId)).ToBacktestJobGroupModel();
        }

        public IActionResult RefreshActiveJobGroups()
        {
            return ViewComponent("JobGroupsList", new { listType = JobGroupListType.Active });
        }

        public IActionResult RefreshInactiveJobGroups()
        {
            return ViewComponent("JobGroupsList", new { listType = JobGroupListType.Inactive });
        }

        public async Task<FileResult> ExportUnrealizedPnLsToExcel(string jobGroupId)
        {
            return await unrealizedPnlSeriesControllerUtils.ExportUnrPnLExcel(jobGroupId);
        }

        public async Task<FileResult> ExportUnrealizedPnLsPerHourToExcel(string jobGroupId)
        {
            return await unrealizedPnlSeriesControllerUtils.ExportUnrPnLPerHourExcel(jobGroupId);
        }

        public async Task<FileResult> ExportGenericMetric2ToExcel(string jobGroupId)
        {
            return await tradeGenericMetric2SeriesControllerUtils.ExportExcel(jobGroupId);
        }
    }
}
