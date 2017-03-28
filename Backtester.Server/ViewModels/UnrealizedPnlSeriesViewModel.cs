using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core;
using Syncfusion.JavaScript.DataVisualization;
using Syncfusion.JavaScript.DataVisualization.Models;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ViewModels
{
    public class UnrealizedPnlSeriesViewModel
    {
        public string JobGroupId { get; private set; }

        public ChartProperties ChartModel { get; private set; } = null;

        public UnrealizedPnlSeriesViewModel(string jobGroupId, IEnumerable<BacktestUnrealizedPnlSerie> series)
        {
            JobGroupId = jobGroupId;

            CreatePnlChart(series);
        }

        private void CreatePnlChart(IEnumerable<BacktestUnrealizedPnlSerie> pnlSeries)
        {
            if (!pnlSeries.IsNullOrEmpty())
            {
                ChartModel = new ChartProperties()
                {
                    Legend = new Legend()
                    {
                        Visible = true
                    },
                    PrimaryXAxis = new Axis()
                    {
                        LabelFormat = "n1",
                        Title = new Title()
                        {
                            Text = "Duration (hours)"
                        }
                    },
                    PrimaryYAxis = new Axis()
                    {
                        RoundingPlaces = 1,
                        Title = new Title()
                        {
                            Text = "Unr Pnl (pips)"
                        }
                    }
                };

                ChartModel.Series.AddRange(pnlSeries.Select(s =>
                {
                    var series = new Series()
                    {
                        Name = s.TradeDescription,
                        Tooltip = new NewTooltip()
                        {
                            Template = "toolTipTemplate",
                            Visible = true
                        },
                        Type = SeriesType.Line
                    };

                    series.Points.AddRange(s.Points.Select(p => new Points()
                    {
                        X = ((double)p.TimeInSeconds / 3600).ToString(),
                        Y = p.UnrealizedPnlInPips
                    }));

                    return series;
                }));
            }
        }
    }
}
