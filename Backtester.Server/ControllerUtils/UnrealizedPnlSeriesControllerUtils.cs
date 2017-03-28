using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Capital.GSG.FX.Data.Core.WebApi;

namespace Backtester.Server.ControllerUtils
{
    public class UnrealizedPnlSeriesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesControllerUtils>();

        private readonly UnrealizedPnlSerieActioner actioner;

        private readonly ConcurrentDictionary<string, List<BacktestUnrealizedPnlSerie>> seriesByJobGroupDict = new ConcurrentDictionary<string, List<BacktestUnrealizedPnlSerie>>();

        public UnrealizedPnlSeriesControllerUtils(UnrealizedPnlSerieActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<GenericActionResult> HandleNewUnrealizedPnlSerie(BacktestUnrealizedPnlSerie pnlSerie)
        {
            try
            {
                if (pnlSerie == null)
                    throw new ArgumentNullException(nameof(pnlSerie));

                if (pnlSerie.Points.IsNullOrEmpty())
                    return new GenericActionResult(false, "Unrealized PnL serie is empty");

                logger.Info($"Received new unrealized PnL serie of {pnlSerie.Points.Count} points for job group {pnlSerie.JobGroupId} (trade {pnlSerie.TradeDescription})");

                // 1. Save in database
                var result = await actioner.Insert(pnlSerie);

                if (!result.Success)
                    return result;

                // 2. Add to in-memory dictionary
                seriesByJobGroupDict.AddOrUpdate(pnlSerie.JobGroupId, (key) =>
                {
                    return new List<BacktestUnrealizedPnlSerie>() { pnlSerie };
                }, (key, oldValue) =>
                {
                    oldValue.Add(pnlSerie);
                    return oldValue;
                });

                return new GenericActionResult(true, $"Successfully added new unrealized PnL serie for job group {pnlSerie.JobGroupId}");
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not processing unrealized PnL serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
            catch (Exception ex)
            {
                string err = "Failed to process unrealized PnL serie";
                logger.Error(err);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }

        internal async Task<List<BacktestUnrealizedPnlSerie>> GetForJobGroup(string jobGroupId)
        {
            List<BacktestUnrealizedPnlSerie> series;

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
