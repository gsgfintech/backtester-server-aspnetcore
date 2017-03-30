using Backtester.Server.Models;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.ControllerUtils
{
    public class WorkersControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<WorkersControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;

        private Dictionary<string, BacktesterWorkerModel> workers = new Dictionary<string, BacktesterWorkerModel>();

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

            if (!workers.ContainsKey(workerName))
            {
                logger.Info($"Registering new backtester worker {workerName}");

                workers.Add(workerName, new BacktesterWorkerModel()
                {
                    Attributes = status.Attributes.ToSystemStatusAttributeModels(),
                    Datacenter = status.Datacenter,
                    IsRunning = status.IsAlive,
                    LastHeardFrom = status.LastHeardFrom,
                    Name = workerName,
                    OverallStatus = status.OverallStatus,
                    StartTime = status.StartTime
                });
            }
            else
            {
                var worker = workers[workerName];

                worker.Attributes = status.Attributes.ToSystemStatusAttributeModels();
                worker.Datacenter = status.Datacenter;
                worker.IsRunning = status.IsAlive;
                worker.LastHeardFrom = status.LastHeardFrom;
                worker.OverallStatus = status.OverallStatus;
                worker.StartTime = status.StartTime;
            }

            return new GenericActionResult(true, $"Updated status of worker {workerName}");
        }
    }
}
