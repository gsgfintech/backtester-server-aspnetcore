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

namespace Backtester.Server.ControllerUtils
{
    public class UnrealizedPnlSeriesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesControllerUtils>();

        private readonly UnrealizedPnlSerieActioner actioner;

        private readonly ConcurrentDictionary<string, BacktestUnrealizedPnlSerie> seriesDict = new ConcurrentDictionary<string, BacktestUnrealizedPnlSerie>();
        private readonly ConcurrentDictionary<string, List<BacktestUnrealizedPnlSerie>> seriesByJobGroupDict = new ConcurrentDictionary<string, List<BacktestUnrealizedPnlSerie>>();

        public UnrealizedPnlSeriesControllerUtils(UnrealizedPnlSerieActioner actioner)
        {
            this.actioner = actioner;
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

        internal async Task<BacktestUnrealizedPnlSerie> Get(string tradeDescription)
        {
            BacktestUnrealizedPnlSerie serie;

            if (seriesDict.TryGetValue(tradeDescription, out serie))
                return serie;
            else
            {
                logger.Info($"Querying serie {tradeDescription} from database as it is not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var result = await actioner.Get(tradeDescription, cts.Token);

                if (result != null)
                    seriesDict.TryAdd(tradeDescription, result);

                return result;
            }
        }
    }
}
