﻿using Backtester.Server.ControllerUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backtester.Server.Controllers.Trades
{
    [Route("api/trades")]
    public class TradesApiController : Controller
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradesApiController>();

        private readonly TradesControllerUtils utils;

        public TradesApiController(TradesControllerUtils utils)
        {
            this.utils = utils;
        }

        [HttpPost]
        public GenericActionResult Post(string backtestJobName, [FromBody]BacktestTrade trade)
        {
            return utils.HandleNewTrade(backtestJobName, trade);
        }
    }
}