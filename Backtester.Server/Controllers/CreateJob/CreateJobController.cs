using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpPost]
        public async Task<IActionResult> Step1Submit(List<IFormFile> files)
        {
            CreateJobStep1ViewModel result;

            var file = files.FirstOrDefault();

            if (file?.Length > 0)
            {
                string filePath = utils.GetFilePath(file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result = utils.ListStratProperties(filePath);
            }
            else
            {
                result = new CreateJobStep1ViewModel()
                {
                    Message = "No content was uploaded",
                    Success = false
                };
            }

            return View("Step1", result);
        }

        public IActionResult Step2(string jobName)
        {
            BacktestJobSettingsModel settings = utils.GetJobSettings(jobName);

            return View(new CreateJobStep2ViewModel(settings));
        }

        [HttpPost]
        public IActionResult Step3(string jobName, CreateJobStep2ViewModel model)
        {
            var settings = utils.SetParameters(jobName, model.Settings.Parameters);

            return View(new CreateJobStep3ViewModel(settings));
        }

        public IActionResult Step4()
        {
            return View(new CreateJobStep4ViewModel());
        }
    }
}
