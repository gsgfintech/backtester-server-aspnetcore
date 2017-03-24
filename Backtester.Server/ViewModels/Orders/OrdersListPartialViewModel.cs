using Backtester.Server.Models;
using Capital.GSG.FX.Utils.Core;
using System.Collections.Generic;
using System.Text;

namespace Backtester.Server.ViewModels.Orders
{
    public class OrdersListPartialViewModel
    {
        public List<BacktestOrderModel> Orders { get; private set; }
        public string JobId { get; private set; }

        public string ClipboardContent { get; private set; }

        public OrdersListPartialViewModel(string jobId, List<BacktestOrderModel> orders)
        {
            JobId = jobId;
            Orders = orders ?? new List<BacktestOrderModel>();

            SetupClipboardContent();
        }

        private void SetupClipboardContent()
        {
            if (!Orders.IsNullOrEmpty())
            {
                StringBuilder sb = new StringBuilder();

                foreach (var order in Orders)
                    sb.AppendLine($"{order.OrderId},{order.PlacedTime},{order.LastUpdateTime},{order.Origin},{order.Status},{order.Type},{order.Side},{order.Quantity},{order.Cross},{order.FillPrice},{order.LimitPrice},{order.StopPrice},{order.TrailingAmount}");

                ClipboardContent = sb.ToString();
            }
            else
                ClipboardContent = string.Empty;
        }
    }
}
