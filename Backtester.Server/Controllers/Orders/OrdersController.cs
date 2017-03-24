using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.Orders
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<OrdersController>();

        private readonly JobsControllerUtils utils;

        public OrdersController(JobsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IActionResult> Index(string jobId, int orderId)
        {
            var job = await utils.Get(jobId);

            if (job?.Output?.Orders?.Count > 0 && job.Output.Orders.ContainsKey(orderId))
                return View(job.Output.Orders[orderId].ToOrderModel());
            else
                return View();
        }
    }
}
