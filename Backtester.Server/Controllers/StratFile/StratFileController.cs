using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.StratFile
{
    [Authorize]
    public class StratFileController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<StratFileController>();

        private CreateJobControllerUtils utils;

        public StratFileController(CreateJobControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            var file = files.FirstOrDefault();

            if (file?.Length > 0)
            {
                string filePath = utils.GetFilePath(file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = utils.ListStratProperties(filePath);

                return View("../JobGroups/Create", result);
            }

            return Ok();
        }
    }
}
