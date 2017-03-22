using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestPositionModel
    {
        public Cross Cross { get; set; }

        [Display(Name = "Position")]
        [DisplayFormat(DataFormatString = "{0:N0}K")]
        public double PositionQuantity { get; set; }

        [Display(Name = "Gross Cumulative PnL (USD)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double? GrossCumulativePnlUsd { get; set; }

        [Display(Name = "Timestamp (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTimeOffset LastUpdate { get; set; }

        [Display(Name = "Cumulative Commission (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double? CumulativeCommissionUsd { get; set; }

        [Display(Name = "Net Cumulative PnL (USD)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double NetCumulativePnlUsd => GrossCumulativePnlUsd ?? 0 - CumulativeCommissionUsd ?? 0;
    }

    public static class PositionModelExtensions
    {
        public static BacktestPositionModel ToPositionModel(this BacktestPosition position)
        {
            if (position == null)
                return null;

            return new BacktestPositionModel()
            {
                Cross = position.Cross,
                CumulativeCommissionUsd = position.CommissionUsd,
                PositionQuantity = position.PositionQuantity / 1000,
                GrossCumulativePnlUsd = position.RealizedPnlUsd,
                LastUpdate = position.LastUpdate.ToLocalTime()
            };
        }

        public static List<BacktestPositionModel> ToPositionModels(this IEnumerable<BacktestPosition> positions)
        {
            return positions?.Select(p => p.ToPositionModel()).ToList();
        }
    }
}
