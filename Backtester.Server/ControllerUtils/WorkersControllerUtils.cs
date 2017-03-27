using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Backtester.Server.ControllerUtils
{
    public class WorkersControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<WorkersControllerUtils>();

        private readonly BacktestWorkerActioner actioner;
        private readonly JobsControllerUtils jobsControllerUtils;

        private Dictionary<string, BacktesterWorkerModel> workers = new Dictionary<string, BacktesterWorkerModel>();

        public WorkersControllerUtils(BacktestWorkerActioner actioner, JobsControllerUtils jobsControllerUtils)
        {
            this.actioner = actioner;
            this.jobsControllerUtils = jobsControllerUtils;
        }

        public async Task<List<BacktesterWorkerModel>> ListWorkers()
        {
            if (workers.IsNullOrEmpty())
                await LoadWorkers();

            return workers?.ToArray().Select(kvp => kvp.Value).OrderBy(w => w.Name).ToList();
        }

        private async Task LoadWorkers()
        {
            var existingWorkers = await actioner.GetAll();

            if (!existingWorkers.IsNullOrEmpty())
                workers = existingWorkers.ToDictionary(w => w.Name, w => w.ToBacktesterWorkerModel());
        }

        internal GenericActionResult<string> RequestJob(string workerName)
        {
            logger.Debug($"Requesting new pending job for {workerName}");

            return jobsControllerUtils.GetNextPendingJobName();
        }

        internal async Task<GenericActionResult> HandleStatusUpdate(string workerName, SystemStatus status)
        {
            if (status == null)
                return new GenericActionResult(false, "Invalid status object: null");

            await LoadWorkers();

            if (!workers.ContainsKey(workerName))
            {
                string err = $"Unable to update status of unknown worker {workerName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }

            var worker = workers[workerName];

            worker.Attributes = status.Attributes.ToSystemStatusAttributeModels();
            worker.Datacenter = status.Datacenter;
            worker.IsRunning = status.IsAlive;
            worker.LastHeardFrom = status.LastHeardFrom;
            worker.OverallStatus = status.OverallStatus;
            worker.StartTime = status.StartTime;

            return new GenericActionResult(true, $"Updated status of worker {workerName}");
        }
    }
}
