using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Utils.Core;
using Syncfusion.JavaScript.DataVisualization.Models;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ViewModels.Jobs
{
    public class JobPositionsViewModel
    {
        public string JobGroupId { get; private set; }
        public string JobId { get; private set; }

        public Dictionary<Cross, List<BacktestPositionModel>> Positions { get; private set; }

        public Dictionary<Cross, ChartProperties> ChartModels { get; private set; } = null;

        public JobPositionsViewModel(string jobGroupId, string jobId, IEnumerable<BacktestPosition> positions)
        {
            JobGroupId = jobGroupId;
            JobId = jobId;

            if (!positions.IsNullOrEmpty())
            {
                Positions = (from p in positions
                             orderby p.LastUpdate
                             group p by p.Cross).ToDictionary(g => g.Key, g => g.ToPositionModels());

                CreateCharts();
            }
            else
                Positions = new Dictionary<Cross, List<BacktestPositionModel>>();
        }

        private void CreateCharts()
        {
            //ChartModels = new Dictionary<Cross, ChartProperties>();

            //foreach (var kvp in Positions)
            //{
            //    var chartModel = new ChartProperties()
            //    {
            //        Legend = new Legend()
            //        {
            //            Visible = true
            //        },
            //        Axes = new List<Axis>()
            //        {
            //            new Axis()
            //            {
            //                LabelFormat = "HH:mm",
            //                Name = "PrimaryX",
            //                Title = new Title()
            //                {
            //                    Text = "Time"
            //                }
            //            },
            //            new Axis()
            //            {
            //                Name = "PrimaryY",
            //                Title = new Title()
            //                {
            //                    Text = "Position"
            //                }
            //            },
            //            new Axis()
            //            {
            //                Name = "SecondarY",
            //                Title = new Title()
            //                {
            //                    Text = "PnL (USD)"
            //                }
            //            }
            //        }
            //    };

            //    chartModel.Series.Add()
            //}
        }
    }
}
