using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.OrderData;
using Capital.GSG.FX.Utils.Core;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobOrdersViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }

        public List<BacktestOrderModel> ActiveOrders { get; private set; }
        public List<BacktestOrderModel> InactiveOrders { get; private set; }

        private readonly OrderStatusCode[] activeStatuses = new OrderStatusCode[2] { OrderStatusCode.PreSubmitted, OrderStatusCode.Submitted };

        public JobOrdersViewModel(string jobGroupId, string jobId, IEnumerable<BacktestOrder> orders)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;

            if (!orders.IsNullOrEmpty())
            {
                ActiveOrders = orders.Where(o => activeStatuses.Contains(o.Status)).OrderByDescending(o => o.PlacedTime).ToOrderModels() ?? new List<BacktestOrderModel>();
                InactiveOrders = orders.Where(o => !activeStatuses.Contains(o.Status)).OrderByDescending(o => o.PlacedTime).ToOrderModels() ?? new List<BacktestOrderModel>();
            }
            else
            {
                ActiveOrders = new List<BacktestOrderModel>();
                InactiveOrders = new List<BacktestOrderModel>();
            }
        }
    }
}
