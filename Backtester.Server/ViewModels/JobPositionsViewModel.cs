using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Utils.Core;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ViewModels
{
    public class JobPositionsViewModel
    {
        public Dictionary<Cross, BacktestPositionModel> Positions { get; private set; }

        public JobPositionsViewModel(IEnumerable<BacktestPosition> positions)
        {
            if (!positions.IsNullOrEmpty())
            {
                var groupings = from p in positions
                                orderby p.LastUpdate
                                group p by p.Cross into g
                                select g;

                Positions = groupings.ToDictionary(g => g.Key, g => new BacktestPositionModel()
                {
                    Cross = g.Key,
                    CumulativeCommissionUsd = g.Select(p => p.CommissionUsd).Sum(),
                    GrossCumulativePnlUsd = g.Select(p => p.RealizedPnlUsd).Sum(),
                    LastUpdate = g.Select(p => p.LastUpdate).Last(),
                    PositionQuantity = g.Select(p => p.PositionQuantity).Last()
                });
            }
            else
                Positions = new Dictionary<Cross, BacktestPositionModel>();
        }
    }
}
