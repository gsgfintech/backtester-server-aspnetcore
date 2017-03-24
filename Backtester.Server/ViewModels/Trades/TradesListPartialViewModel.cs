using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core;
using System.Collections.Generic;
using System.Text;

namespace Backtester.Server.ViewModels.Trades
{
    public class TradesListPartialViewModel
    {
        public string JobGroupId { get; private set; }
        public List<BacktestTradeModel> Trades { get; private set; }

        public string ClipboardContent { get; private set; }

        public TradesListPartialViewModel(string jobGroupId, List<BacktestTradeModel> trades)
        {
            JobGroupId = jobGroupId;
            Trades = trades ?? new List<BacktestTradeModel>();

            SetupClipboardContent();
        }

        private void SetupClipboardContent()
        {
            if (!Trades.IsNullOrEmpty())
            {
                StringBuilder sb = new StringBuilder();

                foreach (var trade in Trades)
                    sb.AppendLine($"{trade.Timestamp},{trade.OrderOrigin},{trade.Side},{trade.Size},{trade.Cross},{trade.Price},{trade.RealizedPnlUsd},{trade.RealizedPnlPips},{trade.Duration},{trade.CommissionUsd}");

                ClipboardContent = sb.ToString();
            }
            else
                ClipboardContent = string.Empty;
        }
    }
}
