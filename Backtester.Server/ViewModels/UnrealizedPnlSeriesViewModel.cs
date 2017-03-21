using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels
{
    public class UnrealizedPnlSeriesViewModel
    {
        public string JobGroupId { get; set; }

        //public List<Series> PnlChartData { get; private set; }
        //public Axis PnlChartXAxis { get; private set; }
        //public Axis PnlChartYAxis { get; private set; }

        //public List<Dictionary<string, double>> DataProvider { get; private set; }

        //public List<Dictionary<string, string>> Graphs { get; private set; }
        public string DataProvider { get; private set; }
        public string Graphs { get; private set; }

        public UnrealizedPnlSeriesViewModel(IEnumerable<BacktestUnrealizedPnlSerie> series)
        {
            //CreateGraphs(series);
            //CreateDataProvider(series);
        }

        //private void CreateDataProvider(IEnumerable<BacktestUnrealizedPnlSerie> series)
        //{
        //    if (!series.IsNullOrEmpty())
        //    {
        //        Dictionary<int, Dictionary<string, double>> pnls = new Dictionary<int, Dictionary<string, double>>();

        //        foreach (var serie in series)
        //        {
        //            string tradeDescription = serie.TradeDescription;

        //            foreach (var point in serie.Points)
        //            {
        //                int timeInSeconds = point.TimeInSeconds;

        //                if (!pnls.ContainsKey(timeInSeconds))
        //                    pnls.Add(timeInSeconds, new Dictionary<string, double>() { { "time", timeInSeconds } });

        //                pnls[timeInSeconds].Add(tradeDescription, point.UnrealizedPnlInPips);
        //            }
        //        }

        //        DataProvider = JsonConvert.SerializeObject(pnls.Values.ToList());
        //    }
        //}

        //private void CreateGraphs(IEnumerable<BacktestUnrealizedPnlSerie> series)
        //{
        //    if (!series.IsNullOrEmpty())
        //    {
        //        var tradeDescriptions = series.Select(s => s.TradeDescription).Distinct();

        //        Graphs = JsonConvert.SerializeObject(tradeDescriptions.Select(td => new Dictionary<string, string>()
        //        {
        //            { "title", td },
        //            { "valueField", td },
        //            { "balloonText", "[[title]]: [[value]]" }
        //        }).ToList());
        //    }
        //}

        //private void CreatePnlChart(IEnumerable<BacktestUnrealizedPnlSerie> series)
        //{
        //    if (!series.IsNullOrEmpty())
        //    {
        //        Tuple<DateTime, DateTime> tradingDayBoundaries = DateTimeUtils.GetTradingDayBoundaries(day);

        //        PnlChartData = new List<Series>();

        //        List<Tuple<DateTime, double>> cumulativePnl = new List<Tuple<DateTime, double>>();

        //        double curPnl = 0;
        //        foreach (var trade in series.AsEnumerable().Reverse())
        //        {
        //            if (trade.RealizedPnlUsd.HasValue)
        //            {
        //                curPnl += trade.RealizedPnlUsd.Value;
        //                cumulativePnl.Add(new Tuple<DateTime, double>(trade.ExecutionTime.LocalDateTime, curPnl));
        //            }
        //        }

        //        PnlChartLData.Add(new Series()
        //        {
        //            DataSource = cumulativePnl,
        //            Marker = new Marker()
        //            {
        //                Shape = ChartShape.Circle,
        //                Visible = true
        //            },
        //            Name = "Cumulative PnL (USD)",
        //            Tooltip = new NewTooltip()
        //            {
        //                Visible = true
        //            },
        //            Type = SeriesType.Line,
        //            XName = "Item1",
        //            XAxisName = "Time (HKT)",
        //            YName = "Item2",
        //            YAxisName = "Cumulative PnL (USD)"
        //        });

        //        PnlChartXAxis = new Axis()
        //        {
        //            IntervalType = ChartIntervalType.Hours,
        //            LabelFormat = "HH:mm",
        //            Range = new Range()
        //            {
        //                Interval = 1,
        //                Max = tradingDayBoundaries.Item2,
        //                Min = tradingDayBoundaries.Item1
        //            },
        //            ValueType = AxisValueType.Double
        //        };

        //        int yUpperBound = !cumulativePnl.IsNullOrEmpty() ? ((int)Math.Max(0, cumulativePnl.Select(i => i.Item2).Max())) + 1 : 100;
        //        int yLowerBound = !cumulativePnl.IsNullOrEmpty() ? ((int)Math.Min(0, cumulativePnl.Select(i => i.Item2).Min())) - 1 : -100;

        //        PnlChartYAxis = new Axis()
        //        {
        //            LabelFormat = "c0",
        //            Range = new Range()
        //            {
        //                Interval = Math.Max(1, (yUpperBound - yLowerBound) / 10),
        //                Max = yUpperBound,
        //                Min = yLowerBound
        //            },
        //            ValueType = AxisValueType.Double
        //        };
        //    }
        //}
    }
}
