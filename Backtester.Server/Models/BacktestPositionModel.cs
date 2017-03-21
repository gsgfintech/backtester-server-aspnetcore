using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestPositionModel
    {
        public Cross Cross { get; set; }

        public double PositionQuantity { get; set; }

        public double? RealizedPnL { get; set; }

        public double? UnrealizedPnL { get; set; }

        public double? RealizedPnlUsd { get; set; }

        public double? UnrealizedPnlUsd { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        public double? CommissionUsd { get; set; }
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
                CommissionUsd = position.CommissionUsd,
                PositionQuantity = position.PositionQuantity,
                RealizedPnL = position.RealizedPnL,
                UnrealizedPnL = position.UnrealizedPnL,
                RealizedPnlUsd = position.RealizedPnlUsd,
                UnrealizedPnlUsd = position.UnrealizedPnlUsd,
                LastUpdate = position.LastUpdate
            };
        }

        public static List<BacktestPositionModel> ToPositionModels(this IEnumerable<BacktestPosition> positions)
        {
            return positions?.Select(p => p.ToPositionModel()).ToList();
        }
    }
}
