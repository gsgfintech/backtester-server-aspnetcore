using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Orders
{
    [Route("api/orders")]
    public class OrdersApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<OrdersApiController>();

        private readonly OrdersControllerUtils utils;

        public OrdersApiController(OrdersControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost]
        public GenericActionResult Post(string backtestJobName, [FromBody]BacktestOrder order)
        {
            return utils.HandleOrderUpdate(backtestJobName, order);
        }
    }
}
