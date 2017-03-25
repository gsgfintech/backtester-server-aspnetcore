using Backtester.Server.ControllerUtils;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.CreateJob
{
    public class CreateJobController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<CreateJobController>();

        private readonly CreateJobControllerUtils utils;

        public CreateJobController(CreateJobControllerUtils utils)
        {
            this.utils = utils;
        }

        public IActionResult Step1()
        {
            return View(new CreateJobStep1ViewModel());
        }

        public IActionResult Step2()
        {
            return View(new CreateJobStep2ViewModel());
        }

        public IActionResult Step3()
        {
            return View(new CreateJobStep3ViewModel());
        }

        public IActionResult Step4()
        {
            return View(new CreateJobStep4ViewModel());
        }
    }
}
