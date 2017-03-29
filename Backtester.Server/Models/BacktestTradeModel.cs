using Backtester.Server.Utils;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.ExecutionData;
using Capital.GSG.FX.Data.Core.OrderData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestTradeModel
    {
        [Display(Name = "ID")]
        public string TradeId { get; set; }

        [Display(Name = "Order ID")]
        public int OrderId { get; set; }

        [Display(Name = "Fees (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double? CommissionUsd { get; set; }

        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}")]
        public TimeSpan? Duration { get; set; }

        [Display(Name = "Rate")]
        public string Price { get; set; }

        [Display(Name = "Realized PnL")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double? RealizedPnL { get; set; }

        [Display(Name = "Realized PnL (pips)")]
        [DisplayFormat(DataFormatString = "{0:N1} pips")]
        public double? RealizedPnlPips { get; set; }

        [Display(Name = "Realized PnL (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double? RealizedPnlUsd { get; set; }

        [Display(Name = "Quantity")]
        [DisplayFormat(DataFormatString = "{0:N0}K")]
        public int Size { get; set; }

        [Display(Name = "Quantity (USD)")]
        [DisplayFormat(DataFormatString = "{0:N0}K")]
        public int? SizeUsd { get; set; }

        [Display(Name = "Execution Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset Timestamp { get; set; }

        public ExecutionSide Side { get; set; }

        [Display(Name = "Pair")]
        public Cross Cross { get; set; }

        [Display(Name = "Origin")]
        public OrderOrigin OrderOrigin { get; set; }

        [Display(Name = "Trade String")]
        public string TradeString => $"{Side} {Size:N0} {Cross} @ {Price:N5} ({Timestamp:dd/MM/yy HH:mm:ss zzz})";
    }

    public static class BacktestTradeModelExtensions
    {
        public static BacktestTradeModel ToTradeModel(this BacktestTrade trade)
        {
            if (trade == null)
                return null;
            else
            {
                var model = new BacktestTradeModel()
                {
                    CommissionUsd = trade.CommissionUsd,
                    Cross = trade.Cross,
                    OrderId = trade.OrderId,
                    OrderOrigin = trade.OrderOrigin,
                    Price = FormatUtils.FormatRate(trade.Cross, trade.Price),
                    RealizedPnL = trade.RealizedPnL,
                    RealizedPnlPips = trade.RealizedPnlPips,
                    RealizedPnlUsd = trade.RealizedPnlUsd,
                    Side = trade.Side,
                    Size = trade.Size / 1000,
                    SizeUsd = trade.SizeUsd.HasValue ? trade.SizeUsd.Value / 1000 : (int?)null,
                    Timestamp = trade.Timestamp.ToLocalTime(),
                    TradeId = trade.TradeId
                };

                TimeSpan duration;

                if (TimeSpan.TryParse(trade.Duration, out duration))
                    model.Duration = duration;

                return model;
            }
        }

        public static List<BacktestTradeModel> ToTradeModels(this IEnumerable<BacktestTrade> trades)
        {
            return trades?.Select(e => e.ToTradeModel()).ToList();
        }

        public static List<BacktestTradeModel> ToTradeModels(this Dictionary<string, BacktestTrade> trades)
        {
            return trades?.Values.ToTradeModels();
        }

        public static bool IsPositionOpening(this BacktestTradeModel trade)
        {
            if (trade == null)
                return false;

            return (trade.OrderOrigin == OrderOrigin.PositionOpen) || (!trade.Duration.HasValue);
        }

        public static bool IsPositionClosing(this BacktestTradeModel trade)
        {
            if (trade == null)
                return false;

            return !trade.IsPositionOpening();
        }

        public static bool IsLong(this BacktestTradeModel trade)
        {
            if (trade == null)
                return false;

            return trade.Side == ExecutionSide.BOUGHT;
        }

        public static bool IsShort(this BacktestTradeModel trade)
        {
            if (trade == null)
                return false;

            return trade.Side == ExecutionSide.SOLD;
        }
    }
}
