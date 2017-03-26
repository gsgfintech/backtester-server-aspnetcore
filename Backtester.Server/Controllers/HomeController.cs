using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backtester.Server.ViewModels;
using Backtester.Server.ControllerUtils;

namespace Backtester.Server.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly JobGroupsControllerUtils jobGroupsControllerUtils;

        public HomeController(JobGroupsControllerUtils jobGroupsControllerUtils)
        {
            this.jobGroupsControllerUtils = jobGroupsControllerUtils;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search()
        {
            var stratsNames = await jobGroupsControllerUtils.ListStratsNames();

            return View(new SearchViewModel(stratsNames));
        }

        [HttpPost]
        public async Task<IActionResult> DoSearch(SearchViewModel settings)
        {
            var stratsNames = await jobGroupsControllerUtils.ListStratsNames();

            if (settings != null)
            {
                settings.SetStratsNames(stratsNames);

                settings.SearchId = await jobGroupsControllerUtils.Search(settings.GroupId, settings.Strategy, settings.RangeStart, settings.RangeEnd);

                return View("Search", settings);
            }
            else
            {
                return View("Search", new SearchViewModel(stratsNames));
            }
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
