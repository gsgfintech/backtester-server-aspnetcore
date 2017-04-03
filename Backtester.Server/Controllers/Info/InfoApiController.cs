using Microsoft.AspNetCore.Mvc;

namespace Backtester.Server.Controllers.Info
{
    [Route("api/info")]
    public class InfoApiController : Controller
    {
        private readonly InfoControllerUtils infoControllerUtils;

        public InfoApiController(InfoControllerUtils infoControllerUtils)
        {
            this.infoControllerUtils = infoControllerUtils;
        }

        [HttpGet]
        public object Get()
        {
            return new { Name = infoControllerUtils.AppName, StartTime = infoControllerUtils.StartTime };
        }
    }
}
