using Backtester.Server.Models;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels.Trades
{
    public class TradesListPartialViewModel
    {
        public string JobGroupId { get; private set; }
        public List<BacktestTradeModel> Trades { get; private set; }

        public TradesListPartialViewModel(string jobGroupId, List<BacktestTradeModel> trades)
        {
            JobGroupId = jobGroupId;
            Trades = trades;
        }
    }
}
