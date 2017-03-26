using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtester.Server.ViewComponents
{
    public class JobGroupsListViewComponent : ViewComponent
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsListViewComponent>();

        private readonly JobGroupsControllerUtils utils;

        public JobGroupsListViewComponent(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IViewComponentResult> InvokeAsync(JobGroupListType listType, string searchId = null)
        {
            ViewViewComponentResult view;

            switch (listType)
            {
                case JobGroupListType.Active:
                    view = View((await utils.GetActiveJobs()).ToBacktestJobGroupModels());
                    view.ViewData["Header"] = "Active Jobs";
                    break;
                case JobGroupListType.Inactive:
                    view = View((await utils.GetInactiveJobs()).ToBacktestJobGroupModels());
                    view.ViewData["Header"] = "Today's Inactive Jobs";
                    break;
                case JobGroupListType.Search:
                    view = !string.IsNullOrEmpty(searchId) ? View(utils.GetSearchResults(searchId).ToBacktestJobGroupModels()) : View(new List<BacktestJobGroupModel>());
                    view.ViewData["Header"] = "Search Results";
                    break;
                default:
                    view = View();
                    break;
            }

            return view;
        }
    }

    public enum JobGroupListType
    {
        Active, Inactive, Search
    }
}
