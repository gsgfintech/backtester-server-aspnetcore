using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capital.GSG.FX.Backtest.DataTypes;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Utils.Core.Logging;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using System.Collections.Concurrent;
using Capital.GSG.FX.Utils.Core;
using System.Threading;

namespace Backtester.Server.ControllerUtils
{
    public class TradeGenericMetric2SeriesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradeGenericMetric2SeriesControllerUtils>();

        private readonly TradeGenericMetric2SerieActioner actioner;

        private readonly ConcurrentDictionary<string, List<BacktestTradeGenericMetric2Serie>> seriesByJobGroupDict = new ConcurrentDictionary<string, List<BacktestTradeGenericMetric2Serie>>();

        public TradeGenericMetric2SeriesControllerUtils(TradeGenericMetric2SerieActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<(bool, string)> HandleNewTradeGenericMetric2Serie(BacktestTradeGenericMetric2Serie tradeGenericMetric2Serie)
        {
            try
            {
                if (tradeGenericMetric2Serie == null)
                    throw new ArgumentNullException(nameof(tradeGenericMetric2Serie));

                if (tradeGenericMetric2Serie.Points.IsNullOrEmpty())
                    return (false, "Unrealized PnL serie is empty");

                logger.Info($"Received new unrealized PnL serie of {tradeGenericMetric2Serie.Points.Count} points for job group {tradeGenericMetric2Serie.JobGroupId} (trade {tradeGenericMetric2Serie.TradeDescription})");

                // 1. Save in database
                var result = await actioner.Insert(tradeGenericMetric2Serie);

                if (!result.Item1)
                    return result;

                // 2. Add to in-memory dictionary
                seriesByJobGroupDict.AddOrUpdate(tradeGenericMetric2Serie.JobGroupId, (key) =>
                {
                    return new List<BacktestTradeGenericMetric2Serie>() { tradeGenericMetric2Serie };
                }, (key, oldValue) =>
                {
                    oldValue.Add(tradeGenericMetric2Serie);
                    return oldValue;
                });

                return (true, $"Successfully added new unrealized PnL serie for job group {tradeGenericMetric2Serie.JobGroupId}");
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not processing unrealized PnL serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return (false, $"{err}: {ex.Message}");
            }
            catch (Exception ex)
            {
                string err = "Failed to process unrealized PnL serie";
                logger.Error(err);
                return (false, $"{err}: {ex.Message}");
            }
        }

        internal async Task<List<BacktestTradeGenericMetric2Serie>> GetForJobGroup(string jobGroupId)
        {
            List<BacktestTradeGenericMetric2Serie> series;

            if (seriesByJobGroupDict.TryGetValue(jobGroupId, out series))
                return series;
            else
            {
                logger.Info($"Querying series for job group {jobGroupId} from database as they are not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var result = await actioner.GetAllForJobGroup(jobGroupId, cts.Token);

                if (!result.IsNullOrEmpty())
                    seriesByJobGroupDict.TryAdd(jobGroupId, result);

                return result;
            }
        }
    }
}
