using Backtester.Server.Models;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels
{
    public class AllTradesViewModel
    {
        public string GroupId { get; set; }

        public List<BacktestTradeModel> Trades { get; set; }
    }
}
