using Capital.GSG.FX.Backtest.DataTypes;
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
    public class JobsConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobsConnector>();

        private readonly string controllerEndpoint;

        private static JobsConnector _instance;

        public static JobsConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new JobsConnector(controllerEndpoint);

            return _instance;
        }

        private JobsConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> UpdateStatus(string backtestJobName, BacktestJob job, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (job == null)
                    throw new ArgumentNullException(nameof(job));

                logger.Info($"About to send GET request to {controllerEndpoint}/api/jobs/update-status/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/jobs/update-status/{backtestJobName}").PostJsonAsync(job, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting job update: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting job update: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post job update";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
