using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using DataTypes.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class JobGroupsControllerUtils : IDisposable
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsControllerUtils>();

        private readonly BacktestJobGroupActioner actioner;
        private readonly JobsControllerUtils jobsControllerUtils;

        private ConcurrentDictionary<string, BacktestJobGroup> activeJobGroups = null;
        private ConcurrentDictionary<string, BacktestJobGroup> inactiveJobGroups = null;
        private bool jobsLoaded = false;
        private object jobsLoadedLocker = new object();

        private List<string> stratsNames = null;

        private ConcurrentDictionary<string, List<BacktestJobGroup>> searchResults = new ConcurrentDictionary<string, List<BacktestJobGroup>>();

        private Timer updateActiveJobGroupsTimer = null;

        public JobGroupsControllerUtils(BacktestJobGroupActioner actioner, JobsControllerUtils jobsControllerUtils)
        {
            this.actioner = actioner;
            this.jobsControllerUtils = jobsControllerUtils;
        }

        private async Task LoadJobGroups(bool reset = false)
        {
            if (updateActiveJobGroupsTimer == null)
                updateActiveJobGroupsTimer = new Timer(RefreshActiveJobGroupsCb, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            if (!jobsLoaded || reset)
            {
                lock (jobsLoadedLocker)
                {
                    jobsLoaded = true;
                }

                await LoadActiveJobGroups();
                await LoadTodaysInactiveJobGroups();
            }
        }

        internal async Task<List<BacktestJobGroup>> GetActiveJobs()
        {
            await LoadJobGroups();

            List<BacktestJobGroup> groups = activeJobGroups?.Values?.ToList();
            groups?.Sort(BacktestJobGroupCreationDateComparer.Instance);

            return groups?.ToList();
        }

        internal async Task<List<BacktestJobGroup>> GetInactiveJobs()
        {
            await LoadJobGroups();

            List<BacktestJobGroup> groups = inactiveJobGroups?.Values?.ToList();
            groups?.Sort(BacktestJobGroupCreationDateComparer.Instance);

            return groups?.ToList();
        }

        internal async Task<BacktestJobGroup> Get(string groupId)
        {
            await LoadJobGroups();

            BacktestJobGroup group;

            if (activeJobGroups.TryGetValue(groupId, out group))
                return group;
            else if (inactiveJobGroups.TryGetValue(groupId, out group))
                return group;
            else
            {
                logger.Info($"Querying backtest job group {groupId} from database as it is not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                return await actioner.Get(groupId, cts.Token);
            }
        }

        internal async Task<List<BacktestTradeModel>> GetTrades(string groupId)
        {
            var jobGroup = await Get(groupId);

            return jobGroup?.Trades.ToTradeModels();
        }

        internal async Task<GenericActionResult> AddJobGroup(BacktestJobGroup group)
        {
            logger.Info($"Adding job group {group.GroupId} to database and active queue");

            var result = await actioner.AddOrUpdate(group.GroupId, group);

            if (!result.Success)
                return result;

            activeJobGroups.TryAdd(group.GroupId, group);

            return new GenericActionResult(true, $"Successfully added job group {group.GroupId}");
        }

        private async Task LoadActiveJobGroups()
        {
            activeJobGroups = new ConcurrentDictionary<string, BacktestJobGroup>();

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            List<BacktestJobGroup> jobGroupsList = await actioner.GetAllActive(cts.Token);

            if (!jobGroupsList.IsNullOrEmpty())
            {
                logger.Debug($"Retrieved {jobGroupsList.Count} active job groups from the database: {string.Join(", ", jobGroupsList.Select(j => j.GroupId))}");

                jobGroupsList.Sort(BacktestJobGroupCreationDateComparer.Instance);

                foreach (var group in jobGroupsList)
                {
                    activeJobGroups.AddOrUpdate(group.GroupId, group, (key, oldValue) => group);
                }
            }
        }

        private async Task LoadTodaysInactiveJobGroups()
        {
            inactiveJobGroups = new ConcurrentDictionary<string, BacktestJobGroup>();

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            List<BacktestJobGroup> jobGroupsList = await actioner.GetTodaysInactive(cts.Token);

            if (!jobGroupsList.IsNullOrEmpty())
            {
                logger.Debug($"Retrieved {jobGroupsList.Count} inactive job groups from the database: {string.Join(", ", jobGroupsList.Select(j => j.GroupId))}");

                jobGroupsList.Sort(BacktestJobGroupCreationDateComparer.Instance);

                foreach (var job in jobGroupsList)
                    inactiveJobGroups.AddOrUpdate(job.GroupId, job, (key, oldValue) => job);
            }
        }

        private async void RefreshActiveJobGroupsCb(object state)
        {
            if (!activeJobGroups.IsEmpty)
            {
                var jobGroups = activeJobGroups.ToArray().Select(kvp => kvp.Value);

                foreach (var jobGroup in jobGroups)
                {
                    try
                    {
                        if (!jobGroup.JobIds.IsNullOrEmpty())
                        {
                            var jobs = await jobsControllerUtils.GetMany(jobGroup.JobIds.Values);

                            if (!jobs.IsNullOrEmpty())
                            {
                                BacktestJobStatus newStatus = jobs.Select(j => j.Status).Min();

                                // Override: if at least one job is in progress then we want to mark the job group as in progress too
                                if (newStatus == BacktestJobStatus.CREATED)
                                {
                                    if (jobs.FirstOrDefault(j => j.Status == BacktestJobStatus.INPROGRESS) != null)
                                        newStatus = BacktestJobStatus.INPROGRESS;
                                }
                                // Override: if at least one job is failed then we want to mark the job group as failed too
                                else if (newStatus == BacktestJobStatus.COMPLETED)
                                {
                                    if (jobs.FirstOrDefault(j => j.Status == BacktestJobStatus.FAILED) != null)
                                        newStatus = BacktestJobStatus.FAILED;
                                }

                                double newProgress = jobs.Select(j => j.Output.Status.Progress).Average();

                                var jobsWithActualStartTime = jobs.Where(j => j.ActualStartTime.HasValue);
                                DateTimeOffset? newActualStartTime = !jobsWithActualStartTime.IsNullOrEmpty() ? jobsWithActualStartTime.Select(j => j.ActualStartTime.Value).Min() : (DateTimeOffset?)null;

                                var jobsWithCompletionTime = jobs.Where(j => j.CompletionTime.HasValue);
                                DateTimeOffset? newCompletionTime = !jobsWithCompletionTime.IsNullOrEmpty() ? jobsWithCompletionTime.Select(j => j.CompletionTime.Value).Max() : (DateTimeOffset?)null;

                                var jobsWithTrades = jobs.Where(j => !j.Output.Trades.IsNullOrEmpty());
                                List<BacktestTrade> newTrades = !jobsWithTrades.IsNullOrEmpty() ? jobsWithTrades.Select(j => j.Output.Trades.Values.ToList()).Aggregate((cur, next) => cur.Concat(next).ToList()).ToList() : null;

                                if (BacktestJobStatusUtils.ActiveStatus.Contains(newStatus))
                                {
                                    activeJobGroups.AddOrUpdate(jobGroup.GroupId, (key) => null, (key, oldValue) =>
                                    {
                                        oldValue.ActualStartTime = newActualStartTime;
                                        oldValue.Progress = newProgress;
                                        oldValue.Status = newStatus;
                                        oldValue.Trades = newTrades;

                                        return oldValue;
                                    });
                                }
                                else
                                {
                                    BacktestJobGroup discarded;
                                    activeJobGroups.TryRemove(jobGroup.GroupId, out discarded);

                                    discarded = inactiveJobGroups.AddOrUpdate(jobGroup.GroupId, (key) =>
                                    {
                                        return new BacktestJobGroup(jobGroup.GroupId)
                                        {
                                            ActualStartTime = newActualStartTime,
                                            CompletionTime = newCompletionTime,
                                            CreateTime = jobGroup.CreateTime,
                                            EndDate = jobGroup.EndDate,
                                            EndTime = jobGroup.EndTime,
                                            JobIds = jobGroup.JobIds,
                                            Progress = newProgress,
                                            StartDate = jobGroup.StartDate,
                                            StartTime = jobGroup.StartTime,
                                            Status = newStatus,
                                            Strategy = jobGroup.Strategy,
                                            Trades = newTrades
                                        };
                                    }, (key, oldValue) =>
                                    {
                                        oldValue.ActualStartTime = newActualStartTime;
                                        oldValue.CompletionTime = newCompletionTime;
                                        oldValue.Progress = newProgress;
                                        oldValue.Status = newStatus;
                                        oldValue.Trades = newTrades;

                                        return oldValue;
                                    });

                                    var dbUpdate = await actioner.AddOrUpdate(jobGroup.GroupId, discarded);

                                    if (!dbUpdate.Success)
                                        logger.Error($"Failed to update job group {jobGroup.GroupId} in database");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to refresh status of job group {jobGroup.GroupId}", ex);
                    }
                }
            }
        }

        internal async Task<GenericActionResult> Delete(string groupId)
        {
            logger.Info($"About to delete job group {groupId} and related subjobs");

            var jobGroup = await Get(groupId);

            if (jobGroup == null)
                return new GenericActionResult(false, $"Unable to delete unknown job group {groupId}");

            if (!jobGroup.JobIds.IsNullOrEmpty())
            {
                var deleteSubjobs = await jobsControllerUtils.DeleteMany(jobGroup.JobIds.Values);

                if (!deleteSubjobs.Success)
                    return new GenericActionResult(false, $"Failed to delete one or more subjob ({string.Join(", ", jobGroup.JobIds)}) for job group {groupId}: {deleteSubjobs.Message}. Not deleting subgroup");
            }

            var result = await actioner.Delete(groupId);

            if (result.Success)
            {
                BacktestJobGroup discarded;
                activeJobGroups.TryRemove(groupId, out discarded);
                inactiveJobGroups.TryRemove(groupId, out discarded);
            }

            return result;
        }

        internal async Task<List<string>> ListStratsNames()
        {
            if (stratsNames == null)
                stratsNames = (await actioner.GetAllStratNames()) ?? new List<string>();

            return stratsNames;
        }

        internal async Task<string> Search(string groupId, string stratName, DateTimeOffset? rangeStart, DateTimeOffset? rangeEnd)
        {
            var results = await actioner.Search(groupId, stratName, null, rangeStart, rangeEnd);

            string searchId = Guid.NewGuid().ToString();

            searchResults.TryAdd(searchId, results);

            return searchId;
        }

        internal List<BacktestJobGroup> GetSearchResults(string searchId)
        {
            List<BacktestJobGroup> results;

            if (searchResults.TryRemove(searchId, out results))
                return results;
            else
                return new List<BacktestJobGroup>();
        }

        public void Dispose()
        {
            try { updateActiveJobGroupsTimer?.Dispose(); updateActiveJobGroupsTimer = null; } catch { }
        }

        private class BacktestJobGroupCreationDateComparer : IComparer<BacktestJobGroup>
        {
            private static BacktestJobGroupCreationDateComparer instance;

            public static BacktestJobGroupCreationDateComparer Instance
            {
                get
                {
                    if (instance == null)
                        instance = new BacktestJobGroupCreationDateComparer();

                    return instance;
                }
            }

            private BacktestJobGroupCreationDateComparer() { }

            public int Compare(BacktestJobGroup x, BacktestJobGroup y)
            {
                return x.CreateTime.CompareTo(y.CreateTime);
            }
        }
    }
}
