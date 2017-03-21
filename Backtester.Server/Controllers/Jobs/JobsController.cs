using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.Jobs
{
    public class JobsController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsController>();

        private readonly JobsControllerUtils utils;

        public JobsController(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public IActionResult Index(string jobId)
        {
            return ViewComponent("Job", jobId);
        }
    }
}
