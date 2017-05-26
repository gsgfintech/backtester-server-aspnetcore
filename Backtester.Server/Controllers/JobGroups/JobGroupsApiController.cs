using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.JobGroups
{
    [Route("api/jobgroups")]
    public class JobGroupsApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsApiController>();

        private readonly JobGroupsControllerUtils utils;

        public JobGroupsApiController(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpDelete("{groupId}")]
        public async Task<GenericActionResult> Delete(string groupId)
        {
            return await utils.Delete(groupId);
        }
    }
}
