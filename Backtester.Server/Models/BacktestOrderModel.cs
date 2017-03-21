using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.OrderData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestOrderModel
    {
        public int OrderId { get; set; }
        public double? TrailingAmount { get; set; }
        public int? UsdQuantity { get; set; }

        public DateTimeOffset? PlacedTime { get; set; }

        public DateTimeOffset? LastUpdateTime { get; set; }

        public Cross Cross { get; set; }

        public OrderSide Side { get; set; }

        public int Quantity { get; set; }

        public OrderType Type { get; set; }

        public double? LimitPrice { get; set; }

        public double? StopPrice { get; set; }

        public OrderStatusCode Status { get; set; }

        public List<OrderHistoryPointModel> History { get; set; }

        public double? FillPrice { get; set; }

        public OrderOrigin Origin { get; set; }
    }

    public class OrderHistoryPointModel
    {
        public DateTimeOffset Timestamp { get; set; }

        public OrderStatusCode Status { get; set; }
    }

    internal static class OrderModelExtensions
    {
        private static OrderHistoryPointModel ToOrderHistoryPointModel(this OrderHistoryPoint point)
        {
            if (point == null)
                return null;

            return new OrderHistoryPointModel()
            {
                Status = point.Status,
                Timestamp = point.Timestamp
            };
        }

        private static List<OrderHistoryPointModel> ToOrderHistoryPointModels(this IEnumerable<OrderHistoryPoint> points)
        {
            return points?.Select(p => p.ToOrderHistoryPointModel()).ToList();
        }

        public static BacktestOrderModel ToOrderModel(this BacktestOrder order)
        {
            if (order == null)
                return null;

            return new BacktestOrderModel()
            {
                Cross = order.Cross,
                FillPrice = order.FillPrice,
                LastUpdateTime = order.LastUpdateTime,
                LimitPrice = order.LimitPrice,
                OrderId = order.OrderId,
                Origin = order.Origin,
                PlacedTime = order.PlacedTime,
                Quantity = order.Quantity,
                Side = order.Side,
                Status = order.Status,
                StopPrice = order.StopPrice,
                Type = order.Type,
                History = order.History.ToOrderHistoryPointModels(),
                TrailingAmount = order.TrailingAmount,
                UsdQuantity = order.UsdQuantity
            };
        }

        public static List<BacktestOrderModel> ToOrderModels(this Dictionary<int, BacktestOrder> orders)
        {
            return orders?.Select(o => o.Value.ToOrderModel()).ToList();
        }
    }
}
