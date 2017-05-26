using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
                    var activeJobs = (await utils.GetActiveJobs()).ToBacktestJobGroupModels();
                    view = View(activeJobs);
                    view.ViewData["Header"] = "Active Jobs";
                    view.ViewData["JsonJobs"] = JsonConvert.SerializeObject(activeJobs);
                    break;
                case JobGroupListType.Inactive:
                    var inactiveJobs = (await utils.GetInactiveJobs()).ToBacktestJobGroupModels();
                    view = View(inactiveJobs);
                    view.ViewData["Header"] = "Today's Inactive Jobs";
                    view.ViewData["JsonJobs"] = JsonConvert.SerializeObject(inactiveJobs);
                    break;
                case JobGroupListType.Search:
                    var searchedJobs = !string.IsNullOrEmpty(searchId) ? utils.GetSearchResults(searchId).ToBacktestJobGroupModels() : new List<BacktestJobGroupModel>();
                    view = View(searchedJobs);
                    view.ViewData["Header"] = "Search Results";
                    view.ViewData["JsonJobs"] = JsonConvert.SerializeObject(searchedJobs);
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
