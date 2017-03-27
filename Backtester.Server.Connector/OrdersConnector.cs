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
    public class OrdersConnector
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<OrdersConnector>();

        private readonly string controllerEndpoint;

        private static OrdersConnector _instance;

        public static OrdersConnector GetConnector(string controllerEndpoint)
        {
            if (_instance == null)
                _instance = new OrdersConnector(controllerEndpoint);

            return _instance;
        }

        private OrdersConnector(string controllerEndpoint)
        {
            if (string.IsNullOrEmpty(controllerEndpoint))
                throw new ArgumentNullException(nameof(controllerEndpoint));

            // In case the address is mistakenly passed with a trailing /
            controllerEndpoint = controllerEndpoint.TrimEnd('/');

            this.controllerEndpoint = controllerEndpoint;
        }

        public async Task<GenericActionResult> PostOrder(string backtestJobName, BacktestOrder order, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(backtestJobName))
                    throw new ArgumentNullException(nameof(backtestJobName));

                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                logger.Info($"About to send POST request to {controllerEndpoint}/api/orders/{backtestJobName}");

                return await controllerEndpoint.AppendPathSegment($"/api/orders/{backtestJobName}").PostJsonAsync(order, ct).ReceiveJson<GenericActionResult>();
            }
            catch (OperationCanceledException)
            {
                string err = "Not posting order update: operation cancelled";
                logger.Warn(err);
                return new GenericActionResult(false, err);
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not posting order update: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, err);
            }
            catch (Exception ex)
            {
                string err = "Failed to post order update";
                logger.Error(err, ex);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }
    }
}
