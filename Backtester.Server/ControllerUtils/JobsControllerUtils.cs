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
using Capital.GSG.FX.Utils.Core;

namespace Backtester.Server.ControllerUtils
{
    public class JobsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsControllerUtils>();

        private readonly BacktestJobActioner actioner;

        private ConcurrentQueue<string> pendingJobs = new ConcurrentQueue<string>();
        //private ConcurrentDictionary<string, BacktestJobStatus> statusesDict = new ConcurrentDictionary<string, BacktestJobStatus>();

        public JobsControllerUtils(BacktestJobActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<BacktestJob> Get(string jobId)
        {
            logger.Info($"Querying backtest job {jobId} from database as it is not in the dictionary");

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            return await actioner.Get(jobId, cts.Token);
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
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            return await actioner.GetMany(jobIds, cts.Token);
        }

        //internal async Task<Dictionary<string, BacktestJobStatus>> GetStatuses(IEnumerable<string> jobIds)
        //{
        //    if (jobIds.IsNullOrEmpty())
        //        return new Dictionary<string, BacktestJobStatus>();

        //    Dictionary<string, BacktestJobStatus> statuses = new Dictionary<string, BacktestJobStatus>();

        //    foreach (var jobId in jobIds)
        //    {
        //        BacktestJobStatus status;
        //        if (!statusesDict.TryGetValue(jobId, out status))
        //            status = (await actioner.Get(jobId))?.Status;

        //        if (status != null)
        //            statuses.Add(jobId, status);
        //    }

        //    return statuses;
        //}

        internal async Task<GenericActionResult> AddJob(BacktestJob job)
        {
            logger.Info($"Adding job {job.Name} to database and active queue");

            var result = await actioner.AddOrUpdate(job.Name, job);

            if (!result.Success)
                return result;

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
                else
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

            return await actioner.DeleteMany(jobNames);
        }

        internal GenericActionResult AddAlert(string jobName, Alert alert)
        {
            //BacktestJob job;

            //if (activeJobs.TryGetValue(jobName, out job))
            //{
            //    job.Output.Alerts.Add(alert);

            //    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

            //    return new GenericActionResult(true, $"Added alert {alert.AlertId} to job {jobName}");
            //}
            //else
            //{
            //    string err = $"Not adding alert to unknown job {jobName}";
            //    logger.Error(err);
            //    return new GenericActionResult(false, err);
            //}

            return new GenericActionResult(false, "Disabled");
        }

        internal GenericActionResult AddPosition(string jobName, BacktestPosition position)
        {
            //BacktestJob job;

            //if (activeJobs.TryGetValue(jobName, out job))
            //{
            //    job.Output.Positions.Add(position);
            //    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

            //    return new GenericActionResult(true, $"Added position update to job {jobName}");
            //}
            //else
            //{
            //    string err = $"Not adding position update to unknown job {jobName}";
            //    logger.Error(err);
            //    return new GenericActionResult(false, err);
            //}

            return new GenericActionResult(false, "Disabled");
        }

        internal GenericActionResult AddTrade(string jobName, BacktestTrade trade)
        {
            //BacktestJob job;

            //if (activeJobs.TryGetValue(jobName, out job))
            //{
            //    if (!job.Output.Trades.ContainsKey(trade.TradeId))
            //        job.Output.Trades.Add(trade.TradeId, trade);
            //    else
            //        job.Output.Trades[trade.TradeId] = trade;

            //    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

            //    return new GenericActionResult(true, $"Added trade {trade.TradeId} to job {jobName}");
            //}
            //else
            //{
            //    string err = $"Not adding trade to unknown job {jobName}";
            //    logger.Error(err);
            //    return new GenericActionResult(false, err);
            //}

            return new GenericActionResult(false, "Disabled");
        }

        internal GenericActionResult AddOrder(string jobName, BacktestOrder order)
        {
            //BacktestJob job;

            //if (activeJobs.TryGetValue(jobName, out job))
            //{
            //    if (!job.Output.Orders.ContainsKey(order.OrderId))
            //        job.Output.Orders.Add(order.OrderId, order);
            //    else
            //        job.Output.Orders[order.OrderId] = order;

            //    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

            //    return new GenericActionResult(true, $"Added order {order.OrderId} to job {jobName}");
            //}
            //else
            //{
            //    string err = $"Not adding order to unknown job {jobName}";
            //    logger.Error(err);
            //    return new GenericActionResult(false, err);
            //}

            return new GenericActionResult(false, "Disabled");
        }

        internal GenericActionResult AddStatusUpdate(string jobName, BacktestJobStatus status)
        {
            //statusesDict.AddOrUpdate(jobName, (key) =>
            //{
            //    logger.Info($"Record status update for new job {jobName}");
            //    return status;
            //}, (key, oldValue) =>
            //{
            //    logger.Debug($"Record status update for job {jobName}");
            //    return status;
            //});

            //if (!statusesDict.ContainsKey(jobName))
            //{
            //    logger.Info($"Record status update for new job {jobName}");
            //    statusesDict.Add(jobName, status);
            //}
            //else
            //{
            //    logger.Debug($"Record status update for job {jobName}");

            //    UpdateAttributes(statusesDict[jobName].Attributes, status.Attributes);

            //    statusesDict[jobName].CompletionTime = status.CompletionTime;
            //    statusesDict[jobName].Message = status.Message;
            //    statusesDict[jobName].Progress = status.Progress;
            //    statusesDict[jobName].StatusCode = status.StatusCode;
            //    statusesDict[jobName].Timestamp = status.Timestamp;
            //}

            return new GenericActionResult(true, "Not used");

            //BacktestJob job;

            //if (activeJobs.TryGetValue(jobName, out job))
            //{
            //    job.Output.Status = status;
            //    activeJobs.AddOrUpdate(jobName, job, (key, oldValue) => job);

            //    return new GenericActionResult(true, $"Updated status of job {jobName}");
            //}
            //else
            //{
            //    string err = $"Not updating status of unknown job {jobName}";
            //    logger.Error(err);
            //    return new GenericActionResult(false, err);
            //}
        }

        private void UpdateAttributes(List<BacktestStatusAttribute> existingAttributes, List<BacktestStatusAttribute> newAttributes)
        {
            if (!newAttributes.IsNullOrEmpty())
            {
                if (existingAttributes.IsNullOrEmpty())
                    existingAttributes = newAttributes;
                else
                {
                    foreach (var newAttribute in newAttributes)
                    {
                        var existingAttribute = existingAttributes.FirstOrDefault(attr => attr.Name == newAttribute.Name);

                        if (existingAttribute != null)
                            existingAttribute.Value = newAttribute.Value;
                        else
                            existingAttributes.Add(newAttribute);
                    }
                }
            }
        }
    }
}
