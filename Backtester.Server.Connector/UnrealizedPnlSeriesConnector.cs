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
    public class UnrealizedPnlSeriesConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesConnector>();

        private readonly string controllerEndpoint;

        private static UnrealizedPnlSeriesConnector _instance;

        public static UnrealizedPnlSeriesConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new UnrealizedPnlSeriesConnector(controllerEndpoint);

            return _instance;
        }

        private UnrealizedPnlSeriesConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostNewUnrealizedPnlSerie(BacktestUnrealizedPnlSerie pnlSerie, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (pnlSerie == null)
                    throw new ArgumentNullException(nameof(pnlSerie));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/unrpnls");

                return await controllerEndpoint.AppendPathSegment("/api/unrpnls").PostJsonAsync(pnlSerie, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting new unrealized PnL serie: operation cancelled";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting new unrealized PnL serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post new unrealized PnL serie";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
