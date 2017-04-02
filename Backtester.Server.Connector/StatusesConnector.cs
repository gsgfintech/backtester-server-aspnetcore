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
    public class StatusesConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<StatusesConnector>();

        private readonly string controllerEndpoint;

        private static StatusesConnector _instance;

        public static StatusesConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new StatusesConnector(controllerEndpoint);

            return _instance;
        }

        private StatusesConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostBacktestJobStatus(string backtestJobName, BacktestJobStatus status, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (status == null)
                    throw new ArgumentNullException(nameof(status));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/statuses/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/statuses/{backtestJobName}").PostJsonAsync(status, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting status update: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting status update: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post status update";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
