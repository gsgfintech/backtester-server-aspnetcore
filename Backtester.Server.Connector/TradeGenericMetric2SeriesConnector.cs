using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core.Logging;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.Connector
{
    public class TradeGenericMetric2SeriesConnector
    {
        private static ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradeGenericMetric2SeriesConnector>();

        private readonly string controllerEndpoint;

        private static TradeGenericMetric2SeriesConnector _instance;

        public static TradeGenericMetric2SeriesConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new TradeGenericMetric2SeriesConnector(controllerEndpoint);

            return _instance;
        }

        private TradeGenericMetric2SeriesConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<(bool, string)> PostNewTradeGenericMetric2Serie(BacktestTradeGenericMetric2Serie tradeGenericMetric2Serie, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (tradeGenericMetric2Serie == null)
                    throw new ArgumentNullException(nameof(tradeGenericMetric2Serie));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/tradegenericmetric2s");

                return await controllerEndpoint.AppendPathSegment("/api/tradegenericmetric2s").PostJsonAsync(tradeGenericMetric2Serie, ct).ReceiveJson<(bool, string)>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting new trade generic metric2 serie: operation cancelled";
                logger.Error(err);
                return (false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting new trade generic metric2 serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return (false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post new trade generic metric2 serie";
                logger.Error(err, ex);
                return (false, $"{err}: {ex.Message}");
            }
        }
    }
}
