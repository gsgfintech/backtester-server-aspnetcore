using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Data.Core.WebApi;
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

        private ConcurrentQueue<string> pendingJobs = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, BacktestJob> activeJobs = new ConcurrentDictionary<string, BacktestJob>();
        private ConcurrentDictionary<string, BacktestJob> inactiveJobs = new ConcurrentDictionary<string, BacktestJob>();

        public JobsControllerUtils(BacktestJobActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<BacktestJob> Get(string jobId)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobId, out job))
                return job;
            else if (inactiveJobs.TryGetValue(jobId, out job))
                return job;
            else
            {
                logger.Info($"Querying backtest job {jobId} from database as it is not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                job = await actioner.Get(jobId, cts.Token);

                if (job != null)
                {
                    if (job.Status == BacktestJobStatus.COMPLETED || job.Status == BacktestJobStatus.FAILED)
                        inactiveJobs.TryAdd(jobId, job);
                    else
                        activeJobs.TryAdd(jobId, job);
                }

                return job;
            }
        }

        internal async Task<GenericActionResult> AddJob(BacktestJob job)
        {
            logger.Info($"Adding job {job.Name} to database and active queue");

            var result = await actioner.AddOrUpdate(job.Name, job);

            if (!result.Success)
                return result;

            activeJobs.TryAdd(job.Name, job);
            pendingJobs.Enqueue(job.Name);

            return new GenericActionResult(true, $"Successfully added job {job.Name}");
        }

        internal async Task<GenericActionResult> DeleteMany(IEnumerable<string> jobNames)
        {
            logger.Info($"About to delete {jobNames.Count()} jobs from DB: {string.Join(", ", jobNames)}");

            var result = await actioner.DeleteMany(jobNames);

            if (result.Success)
            {
                foreach (var jobName in jobNames)
                {
                    BacktestJob discarded;
                    activeJobs.TryRemove(jobName, out discarded);
                    inactiveJobs.TryRemove(jobName, out discarded);
                }
            }

            return result;
        }
    }
}
