using Backtester.Server.Models;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class WorkersControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<WorkersControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        private ConcurrentDictionary<string, BacktesterWorkerModel> workers = new ConcurrentDictionary<string, BacktesterWorkerModel>();

        public WorkersControllerUtils(JobsControllerUtils jobsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
        }

        public List<BacktesterWorkerModel> ListWorkers()
        {
            return workers?.ToArray().Select(kvp => kvp.Value).OrderBy(w => w.Name).ToList();
        }

        internal GenericActionResult<string> RequestJob(string workerName)
        {
            logger.Debug($"Requesting new pending job for {workerName}");

            return jobsControllerUtils.GetNextPendingJobName();
        }

        internal GenericActionResult HandleStatusUpdate(string workerName, SystemStatus status)
        {
            if (status == null)
                return new GenericActionResult(false, "Invalid status object: null");

            workers.AddOrUpdate(workerName, (key) =>
            {
                logger.Info($"Registering new backtester worker {workerName}");

                return new BacktesterWorkerModel()
                {
                    Attributes = status.Attributes.ToSystemStatusAttributeModels(),
                    Datacenter = status.Datacenter,
                    IsRunning = status.IsAlive,
                    LastHeardFrom = status.LastHeardFrom,
                    Name = workerName,
                    OverallStatus = status.OverallStatus,
                    StartTime = status.StartTime
                };
            }, (key, oldValue) =>
            {
                oldValue.Attributes = status.Attributes.ToSystemStatusAttributeModels();
                oldValue.Datacenter = status.Datacenter;
                oldValue.IsRunning = status.IsAlive;
                oldValue.LastHeardFrom = status.LastHeardFrom;
                oldValue.OverallStatus = status.OverallStatus;
                oldValue.StartTime = status.StartTime;

                return oldValue;
            });

            return new GenericActionResult(true, $"Updated status of worker {workerName}");
        }

        internal async Task<(bool Success, string Message)> AcceptJobs(string workerName)
        {
            await Task.CompletedTask;

            return (true, "Not yet implemented");
        }

        internal async Task<(bool Success, string Message)> RejectJobs(string workerName)
        {
            await Task.CompletedTask;

            return (true, "Not yet implemented");
        }

        internal async Task<(bool Success, string Message)> Start(string workerName)
        {
            await Task.CompletedTask;

            return (true, "Not yet implemented");
        }

        internal async Task<(bool Success, string Message)> Stop(string workerName)
        {
            await Task.CompletedTask;

            return (true, "Not yet implemented");
        }
    }
}
