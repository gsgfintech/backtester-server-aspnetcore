using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
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
    public class JobGroupsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsControllerUtils>();

        private readonly BacktestJobGroupActioner actioner;

        private ConcurrentQueue<string> pendingJobGroups = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, BacktestJobGroup> activeJobGroups = null;
        private ConcurrentDictionary<string, BacktestJobGroup> inactiveJobGroups = null;
        private bool jobsLoaded = false;
        private object jobsLoadedLocker = new object();

        public JobGroupsControllerUtils(BacktestJobGroupActioner actioner)
        {
            this.actioner = actioner;
        }

        private async Task LoadJobGroups(bool reset = false)
        {
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

        private async Task LoadActiveJobGroups()
        {
            activeJobGroups = new ConcurrentDictionary<string, BacktestJobGroup>();
            pendingJobGroups = new ConcurrentQueue<string>();

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

                    if (group.Status == BacktestJobStatus.CREATED)
                        pendingJobGroups.Enqueue(group.GroupId);
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
