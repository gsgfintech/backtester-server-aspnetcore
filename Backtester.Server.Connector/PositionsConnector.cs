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
    public class PositionsConnector
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<PositionsConnector>();

        private readonly string controllerEndpoint;

        private static PositionsConnector _instance;

        public static PositionsConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new PositionsConnector(controllerEndpoint);

            return _instance;
        }

        private PositionsConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostPosition(string backtestJobName, BacktestPosition position, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (position == null)
                    throw new ArgumentNullException(nameof(position));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/positions/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/positions/{backtestJobName}").PostJsonAsync(position, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting position update: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting position update: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post position update";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
