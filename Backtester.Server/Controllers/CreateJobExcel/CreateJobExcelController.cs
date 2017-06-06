using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJobExcel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.CreateJobExcel
{
    public class CreateJobExcelController : Controller
    {
        private readonly CreateJobExcelControllerUtils utils;

        public CreateJobExcelController(CreateJobExcelControllerUtils utils)
        {
            this.utils = utils;
        }

        public IActionResult Step1()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Step1Submit(List<IFormFile> files)
        {
            (bool Success, string Message, List<BacktestJobSettingsModel> JobsSettings) result;

            var file = files.FirstOrDefault();

            if (file?.Length > 0)
            {
                string filePath = utils.GetFilePath(file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var readFileResult = utils.ReadFile(filePath);

                result = readFileResult;
            }
            else
                result = (false, "No content was uploaded", null);

            if (!result.Success)
                return View("Step1", new CreateJobExcelStep1ViewModel()
                {
                    Message = result.Message,
                    Success = result.Success
                });
            else
                return View("Review", new CreateJobExcelReviewViewModel()
                {
                    JobsSettings = result.JobsSettings,
                    Message = result.Message
                });
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody]List<string> data)
        {
            var result = await utils.CreateJobs(data);

            return PartialView(new CreateJobExcelSubmitViewModel()
            {
                JobNames = result.JobNames,
                Message = result.Message,
                Success = result.Success
            });
        }
    }
}
