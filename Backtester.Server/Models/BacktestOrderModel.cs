using static Backtester.Server.Utils.FormatUtils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.OrderData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestOrderModel
    {
        [Display(Name = "ID")]
        public int OrderId { get; set; }

        [Display(Name = "Trail Amt")]
        public string TrailingAmount { get; set; }

        public int? UsdQuantity { get; set; }

        [Display(Name = "Placed Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTimeOffset? PlacedTime { get; set; }

        [Display(Name = "Last Update (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTimeOffset? LastUpdateTime { get; set; }

        [Display(Name = "Pair")]
        public Cross Cross { get; set; }

        public OrderSide Side { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}K")]
        public double Quantity { get; set; }

        public OrderType Type { get; set; }

        [Display(Name = "Limit Price")]
        public string LimitPrice { get; set; }

        [Display(Name = "Stop Price")]
        public string StopPrice { get; set; }

        public OrderStatusCode Status { get; set; }

        public List<OrderHistoryPointModel> History { get; set; }

        [Display(Name = "Fill Price")]
        public string FillPrice { get; set; }

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
                Timestamp = point.Timestamp.ToLocalTime()
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
                FillPrice = FormatRate(order.Cross, order.FillPrice),
                LastUpdateTime = order.LastUpdateTime,
                LimitPrice = FormatRate(order.Cross, order.LimitPrice),
                OrderId = order.OrderId,
                Origin = order.Origin,
                PlacedTime = order.PlacedTime,
                Quantity = order.Quantity / 1000,
                Side = order.Side,
                Status = order.Status,
                StopPrice = FormatRate(order.Cross, order.StopPrice),
                Type = order.Type,
                History = order.History.ToOrderHistoryPointModels(),
                TrailingAmount = FormatRate(order.Cross, order.TrailingAmount),
                UsdQuantity = order.UsdQuantity
            };
        }

        public static List<BacktestOrderModel> ToOrderModels(this Dictionary<int, BacktestOrder> orders)
        {
            return orders?.Select(o => o.Value.ToOrderModel()).ToList();
        }

        public static List<BacktestOrderModel> ToOrderModels(this IEnumerable<BacktestOrder> orders)
        {
            return orders?.Select(o => o.ToOrderModel()).ToList();
        }
    }
}
