using Backtester.Server.Models;
using System.Collections.Generic;

namespace Backtester.Server.ViewModels.Orders
{
    public class OrdersListPartialViewModel
    {
        public List<BacktestOrderModel> Orders { get; private set; }
        public string JobId { get; private set; }

        public OrdersListPartialViewModel(string jobId, List<BacktestOrderModel> orders)
        {
            JobId = jobId;
            Orders = orders ?? new List<BacktestOrderModel>();
        }
    }
}
