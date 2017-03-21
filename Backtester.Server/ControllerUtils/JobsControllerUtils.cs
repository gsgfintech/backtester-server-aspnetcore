using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class JobsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsControllerUtils>();

        private readonly BacktestJobActioner actioner;

        private ConcurrentDictionary<string, BacktestJob> jobs = new ConcurrentDictionary<string, BacktestJob>();

        public JobsControllerUtils(BacktestJobActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<BacktestJob> Get(string jobId)
        {
            BacktestJob job;

            if (jobs.TryGetValue(jobId, out job))
                return job;
            else
            {
                logger.Info($"Querying backtest job {jobId} from database as it is not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                job = await actioner.Get(jobId, cts.Token);

                if (job != null && (job.Status == BacktestJobStatus.COMPLETED || job.Status == BacktestJobStatus.FAILED))
                    jobs.TryAdd(jobId, job);

                return job;
            }
        }
    }
}
