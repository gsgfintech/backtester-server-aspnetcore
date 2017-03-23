using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.Trades
{
    [Authorize]
    public class TradesController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradesController>();

        private readonly JobGroupsControllerUtils utils;

        public TradesController(JobGroupsControllerUtils utils)
        {
            this.utils = utils;
        }

        public async Task<IActionResult> Index(string jobGroupId, string tradeId)
        {
            var jobGroup = await utils.Get(jobGroupId);

            return View(jobGroup?.Trades?.FirstOrDefault(t => t.TradeId == tradeId).ToTradeModel());
        }
    }
}
