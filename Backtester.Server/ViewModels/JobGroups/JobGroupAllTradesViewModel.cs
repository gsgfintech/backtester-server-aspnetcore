using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels.JobGroups
{
    public class JobGroupAllTradesViewModel
    {
        public string JobGroupId { get; set; }
        public List<BacktestTradeModel> Trades { get; private set; }

        public JobGroupAllTradesViewModel(string jobGroupId, IEnumerable<BacktestTrade> trades)
        {
            JobGroupId = jobGroupId;

            Trades = trades.ToTradeModels();
        }
    }
}
