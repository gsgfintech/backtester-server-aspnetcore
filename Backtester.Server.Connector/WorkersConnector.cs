using Capital.GSG.FX.Data.Core.SystemData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core.Logging;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.Connector
{
    public class WorkersConnector
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<WorkersConnector>();

        private readonly string controllerEndpoint;

        private static WorkersConnector _instance;

        public static WorkersConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new WorkersConnector(controllerEndpoint);

            return _instance;
        }

        private WorkersConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostStatus(string workerName, SystemStatus status, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(workerName))
                    throw new ArgumentNullException(nameof(workerName));

                if (status == null)
                    throw new ArgumentNullException(nameof(status));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/workers/status/{workerName}");

                return await controllerEndpoint.AppendPathSegment($"/api/workers/status/{workerName}").PostJsonAsync(status, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting worker status update: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting worker status update: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post worker status update";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }

        public async Task<GenericActionResult<string>> RequestNewJob(string workerName, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(workerName))
                    throw new ArgumentNullException(nameof(workerName));

                logger.Info($"About to send GET request to {controllerEndpoint}/api/workers/{workerName}/request-job");

                return await controllerEndpoint.AppendPathSegment($"/api/workers/{workerName}/request-job").GetJsonAsync<GenericActionResult<string>>(ct);
            }
            catch (OperationCanceledException)
            {
                string err = "Not requesting new job: operation cancelled";
                logger.Error(err);
                return new GenericActionResult<string>(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not requesting new job: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult<string>(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to request new job";
                logger.Error(err, ex);
                return new GenericActionResult<string>(false, $"{err}: {ex.Message}");
            }
        }
    }
}
