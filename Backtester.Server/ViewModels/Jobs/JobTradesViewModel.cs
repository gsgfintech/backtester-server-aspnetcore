using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobTradesViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }

        public List<BacktestTradeModel> Trades { get; private set; }

        public JobTradesViewModel(string jobGroupId, string jobId, IEnumerable<BacktestTrade> trades)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;

            Trades = trades.ToTradeModels() ?? new List<BacktestTradeModel>();
        }
    }
}
