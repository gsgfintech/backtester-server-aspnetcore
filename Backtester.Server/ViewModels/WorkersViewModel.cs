using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels
{
    public class WorkersViewModel
    {
        public List<BacktesterWorkerModel> Workers { get; set; }

        public WorkersViewModel() { }

        public WorkersViewModel(List<BacktesterWorkerModel> workers)
        {
            Workers = workers ?? new List<BacktesterWorkerModel>();
        }
    }
}
