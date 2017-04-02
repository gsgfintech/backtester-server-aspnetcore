using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.OrderData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestUnrealizedPnlSerieModel
    {
        public string TradeDescription { get; set; }

        public OrderOrigin TradeCloseOrigin { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public List<BacktestUnrealizedPnlPointModel> Points { get; set; }
    }

    public class BacktestUnrealizedPnlPointModel
    {
        public int TimeInSeconds { get; set; }

        public double UnrealizedPnlInPips { get; set; }
    }

    internal static class BacktestUnrealizedPnlSerieModelExtensions
    {
        private static BacktestUnrealizedPnlPointModel ToBacktestUnrealizedPnlPointModel(this BacktestUnrealizedPnlPoint point)
        {
            if (point == null)
                return null;

            return new BacktestUnrealizedPnlPointModel()
            {
                TimeInSeconds = point.TimeInSeconds,
                UnrealizedPnlInPips = point.UnrealizedPnlInPips
            };
        }

        private static List<BacktestUnrealizedPnlPointModel> ToBacktestUnrealizedPnlPointModels(this IEnumerable<BacktestUnrealizedPnlPoint> points)
        {
            return points?.Select(p => p.ToBacktestUnrealizedPnlPointModel()).ToList();
        }

        public static BacktestUnrealizedPnlSerieModel ToBacktestUnrealizedPnlSerieModel(this BacktestUnrealizedPnlSerie serie, DateTimeOffset? timestamp = null)
        {
            if (serie == null)
                return null;

            return new BacktestUnrealizedPnlSerieModel()
            {
                Points = serie.Points.ToBacktestUnrealizedPnlPointModels(),
                Timestamp = timestamp,
                TradeCloseOrigin = serie.TradeCloseOrigin,
                TradeDescription = serie.TradeDescription
            };
        }

        public static List<BacktestUnrealizedPnlSerieModel> ToBacktestUnrealizedPnlSerieModels(this IEnumerable<BacktestUnrealizedPnlSerie> series)
        {
            return series?.Select(s => s.ToBacktestUnrealizedPnlSerieModel()).ToList();
        }

        private static BacktestUnrealizedPnlPoint ToBacktestUnrealizedPnlPoint(this BacktestUnrealizedPnlPointModel point)
        {
            if (point == null)
                return null;

            return new BacktestUnrealizedPnlPoint()
            {
                TimeInSeconds = point.TimeInSeconds,
                UnrealizedPnlInPips = point.UnrealizedPnlInPips
            };
        }

        private static List<BacktestUnrealizedPnlPoint> ToBacktestUnrealizedPnlPoints(this IEnumerable<BacktestUnrealizedPnlPointModel> points)
        {
            return points?.Select(p => p.ToBacktestUnrealizedPnlPoint()).ToList();
        }

        private static BacktestUnrealizedPnlSerie ToBacktestUnrealizedPnlSerie(this BacktestUnrealizedPnlSerieModel serie)
        {
            if (serie == null)
                return null;

            return new BacktestUnrealizedPnlSerie()
            {
                Points = serie.Points.ToBacktestUnrealizedPnlPoints(),
                TradeDescription = serie.TradeDescription
            };
        }

        public static List<BacktestUnrealizedPnlSerie> ToBacktestUnrealizedPnlSeries(this IEnumerable<BacktestUnrealizedPnlSerieModel> series)
        {
            return series?.Select(s => s.ToBacktestUnrealizedPnlSerie()).ToList();
        }
    }
}
