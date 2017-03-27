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
    public class AlertsConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<AlertsConnector>();

        private readonly string controllerEndpoint;

        private static AlertsConnector _instance;

        public static AlertsConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new AlertsConnector(controllerEndpoint);

            return _instance;
        }

        private AlertsConnector(string controllermonitoringEndpoint)
        {
            if (string.IsNullOrEmpty(controllermonitoringEndpoint))
                throw new ArgumentNullException(nameof(controllermonitoringEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllermonitoringEndpoint = controllermonitoringEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllermonitoringEndpoint;
        }

        public async Task<GenericActionResult> PostNewAlert(string backtestJobName, Alert alert, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (alert == null)
                    throw new ArgumentNullException(nameof(alert));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/alerts/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/alerts/{backtestJobName}").PostJsonAsync(alert, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting new alert: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting new alert: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post new alert";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
