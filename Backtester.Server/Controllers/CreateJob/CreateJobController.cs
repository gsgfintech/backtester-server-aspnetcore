using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.CreateJob
{
    public class CreateJobController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<CreateJobController>();

        private readonly CreateJobControllerUtils createJobControllerUtils;
        private readonly JobGroupsControllerUtils jobGroupsControllerUtils;

        public CreateJobController(CreateJobControllerUtils createJobControllerUtils, JobGroupsControllerUtils jobGroupsControllerUtils)
        {
            this.createJobControllerUtils = createJobControllerUtils;
            this.jobGroupsControllerUtils = jobGroupsControllerUtils;
        }

        public async Task<IActionResult> Step1(string jobNameToDuplicate)
        {
            if (string.IsNullOrEmpty(jobNameToDuplicate))
            {
                logger.Info("Will create a new job");
                return View(new CreateJobStep1ViewModel());
            }
            else
                return View(await createJobControllerUtils.DuplicateJob(jobNameToDuplicate));
        }

        [HttpPost]
        public async Task<IActionResult> Step1Submit(List<IFormFile> files)
        {
            CreateJobStep1ViewModel result;

            var file = files.FirstOrDefault();

            if (file?.Length > 0)
            {
                string filePath = createJobControllerUtils.GetFilePath(file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result = createJobControllerUtils.ListStratProperties(filePath);
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
            BacktestJobSettingsModel settings = createJobControllerUtils.GetJobSettings(jobName);

            return View(new CreateJobStep2ViewModel(settings));
        }

        [HttpPost]
        public IActionResult Step3(string jobName, CreateJobStep2ViewModel model)
        {
            var settings = createJobControllerUtils.SetParameters(jobName, model.Settings.Parameters);

            return View(new CreateJobStep3ViewModel(settings));
        }

        public IActionResult Step4(string jobName, CreateJobStep3ViewModel model)
        {
            var settings = createJobControllerUtils.SetTimeRange(jobName, model.Settings.StartDate, model.Settings.EndDate, model.Settings.StartTime, model.Settings.EndTime, model.Settings.UseHistoDatabase);

            return View(new CreateJobStep4ViewModel(settings));
        }

        public async Task<IActionResult> Submit(string jobName)
        {
            var result = await createJobControllerUtils.CreateJob(jobName);

            return View(new CreateJobSubmitViewModel()
            {
                JobName = result.JobName,
                Message = result.Message,
                Success = result.Success
            });
        }
    }
}
