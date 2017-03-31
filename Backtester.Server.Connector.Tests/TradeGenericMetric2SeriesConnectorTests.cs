using Capital.GSG.FX.Backtest.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class TradeGenericMetric2SeriesConnectorTests
    {
        [TestMethod]
        public async Task TestPostNewTradeGenericMetric2Serie()
        {
            string controllerEndpoint = "https://localhost:44379/";

            TradeGenericMetric2SeriesConnector connector = TradeGenericMetric2SeriesConnector.GetConnector(controllerEndpoint);

            try
            {
                string jobGroupId = Guid.NewGuid().ToString();
                string tradeDescription = "Trade 1";

                var serie = new BacktestTradeGenericMetric2Serie()
                {
                    JobGroupId = jobGroupId,
                    Points = new List<BacktestTradeGenericMetric2Point>()
                    {
                        new BacktestTradeGenericMetric2Point() { TimeInSeconds = 0, GenericMetric2Value = 1.5 },
                        new BacktestTradeGenericMetric2Point() { TimeInSeconds = 1, GenericMetric2Value = 2.5 },
                        new BacktestTradeGenericMetric2Point() { TimeInSeconds = 2, GenericMetric2Value = 3.5 },
                        new BacktestTradeGenericMetric2Point() { TimeInSeconds = 3, GenericMetric2Value = 4.5 },
                        new BacktestTradeGenericMetric2Point() { TimeInSeconds = 4, GenericMetric2Value = 5.5 },
                    },
                    TradeDescription = tradeDescription
                };

                var result = await connector.PostNewTradeGenericMetric2Serie(serie);

                Assert.IsTrue(result.Item1, result.Item2);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
