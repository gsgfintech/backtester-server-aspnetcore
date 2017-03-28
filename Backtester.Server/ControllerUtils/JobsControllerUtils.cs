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
using Capital.GSG.FX.Data.Core.SystemData;

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

        internal GenericActionResult<string> GetNextPendingJobName()
        {
            if (pendingJobs.IsEmpty)
            {
                string msg = "No pending job in the queue";
                logger.Debug(msg);
                return new GenericActionResult<string>(true, msg, null);
            }

            string jobName;
            if (pendingJobs.TryDequeue(out jobName))
                return new GenericActionResult<string>(true, null, jobName);
            else
            {
                logger.Error("Failed to dequeue a pending job");
                return new GenericActionResult<string>(false, "Failed to dequeue a pending job");
            }
        }

        internal async Task<List<BacktestJob>> GetMany(IEnumerable<string> jobIds)
        {
            List<BacktestJob> jobs = new List<BacktestJob>();

            foreach (var jobId in jobIds)
            {
                BacktestJob job;

                if (activeJobs.TryGetValue(jobId, out job))
                {
                    jobs.Add(job);
                    continue;
                }
                else if (inactiveJobs.TryGetValue(jobId, out job))
                {
                    jobs.Add(job);
                    continue;
                }
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

                    jobs.Add(job);
                }
            }

            return jobs;
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

        internal async Task<GenericActionResult> UpdateJob(string jobName, BacktestJob job)
        {
            try
            {
                logger.Info($"Updating job {jobName} in database and dictionary");

                var result = await actioner.AddOrUpdate(jobName, job);

                if (!result.Success)
                    return result;

                if (job.Status == BacktestJobStatus.CREATED || job.Status == BacktestJobStatus.INPROGRESS)
                    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) =>
                    {
                        oldValue.ActualStartTime = job.ActualStartTime;
                        oldValue.CompletionTime = job.CompletionTime;
                        oldValue.Status = job.Status;
                        oldValue.Worker = job.Worker;

                        return oldValue;
                    });
                else
                {
                    BacktestJob discarded;
                    activeJobs.TryRemove(jobName, out discarded);

                    inactiveJobs.AddOrUpdate(jobName, job, (key, oldValue) =>
                    {
                        oldValue.ActualStartTime = job.ActualStartTime;
                        oldValue.CompletionTime = job.CompletionTime;
                        oldValue.Output = job.Output;
                        oldValue.Status = job.Status;
                        oldValue.Worker = job.Worker;

                        return oldValue;
                    });
                }

                return new GenericActionResult(true, $"Successfully updated job {job.Name}");
            }
            catch (Exception ex)
            {
                string err = $"Failed to update job {job?.Name}";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
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

        internal GenericActionResult AddAlert(string jobName, Alert alert)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobName, out job))
            {
                job.Output.Alerts.Add(alert);

                activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

                return new GenericActionResult(true, $"Added alert {alert.AlertId} to job {jobName}");
            }
            else
            {
                string err = $"Not adding alert to unknown job {jobName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
        }

        internal GenericActionResult AddPosition(string jobName, BacktestPosition position)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobName, out job))
            {
                job.Output.Positions.Add(position);
                activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

                return new GenericActionResult(true, $"Added position update to job {jobName}");
            }
            else
            {
                string err = $"Not adding position update to unknown job {jobName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
        }

        internal GenericActionResult AddTrade(string jobName, BacktestTrade trade)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobName, out job))
            {
                if (!job.Output.Trades.ContainsKey(trade.TradeId))
                    job.Output.Trades.Add(trade.TradeId, trade);
                else
                    job.Output.Trades[trade.TradeId] = trade;

                activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

                return new GenericActionResult(true, $"Added trade {trade.TradeId} to job {jobName}");
            }
            else
            {
                string err = $"Not adding trade to unknown job {jobName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
        }

        internal GenericActionResult AddOrder(string jobName, BacktestOrder order)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobName, out job))
            {
                if (!job.Output.Orders.ContainsKey(order.OrderId))
                    job.Output.Orders.Add(order.OrderId, order);
                else
                    job.Output.Orders[order.OrderId] = order;

                activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

                return new GenericActionResult(true, $"Added order {order.OrderId} to job {jobName}");
            }
            else
            {
                string err = $"Not adding order to unknown job {jobName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
        }

        internal GenericActionResult AddStatusUpdate(string jobName, BacktestStatus status)
        {
            BacktestJob job;

            if (activeJobs.TryGetValue(jobName, out job))
            {
                job.Output.Status = status;
                activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

                return new GenericActionResult(true, $"Updated status of job {jobName}");
            }
            else
            {
                string err = $"Not updating status of unknown job {jobName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
        }
    }
}
