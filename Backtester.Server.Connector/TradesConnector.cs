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
    public class TradesConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradesConnector>();

        private readonly string controllerEndpoint;

        private static TradesConnector _instance;

        public static TradesConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new TradesConnector(controllerEndpoint);

            return _instance;
        }

        private TradesConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostTrade(string backtestJobName, BacktestTrade trade, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (trade == null)
                    throw new ArgumentNullException(nameof(trade));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/executions/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/trades/{backtestJobName}").PostJsonAsync(trade, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting trade: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting new trade: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post new trade";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
