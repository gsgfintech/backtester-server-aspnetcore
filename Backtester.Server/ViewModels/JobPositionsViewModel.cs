using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels
{
    public class JobPositionsViewModel
    {
        public Dictionary<Cross, BacktestPositionModel> Positions { get; private set; }

        public JobPositionsViewModel(IEnumerable<BacktestPosition> positions)
        {
            if (!positions.IsNullOrEmpty())
            {
                var groupings = positions.GroupBy(p => p.Cross);

                Positions = groupings.ToDictionary(g=>g.Key, g=> new BacktestPositionModel()
                {
                    Cross=g.Key,
                    CumulativeCommissionUsd =g.
                })
            }
            else
                Positions = new Dictionary<Cross, BacktestPositionModel>();
        }
    }
}
