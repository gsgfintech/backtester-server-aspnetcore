using Capital.GSG.FX.Backtest.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class UnrealizedPnlSeriesConnectorTests
    {
        [TestMethod]
        public async Task TestPostNewUnrealizedPnLSerie()
        {
            string controllerEndpoint = "https://localhost:44379/";

            UnrealizedPnlSeriesConnector connector = UnrealizedPnlSeriesConnector.GetConnector(controllerEndpoint);

            try
            {
                string jobGroupId = Guid.NewGuid().ToString();
                string tradeDescription = "Trade 1";

                var serie = new BacktestUnrealizedPnlSerie()
                {
                    JobGroupId = jobGroupId,
                    Points = new List<BacktestUnrealizedPnlPoint>()
                    {
                        new BacktestUnrealizedPnlPoint() { TimeInSeconds = 0, UnrealizedPnlInPips = 1.5 },
                        new BacktestUnrealizedPnlPoint() { TimeInSeconds = 1, UnrealizedPnlInPips = 2.5 },
                        new BacktestUnrealizedPnlPoint() { TimeInSeconds = 2, UnrealizedPnlInPips = 3.5 },
                        new BacktestUnrealizedPnlPoint() { TimeInSeconds = 3, UnrealizedPnlInPips = 4.5 },
                        new BacktestUnrealizedPnlPoint() { TimeInSeconds = 4, UnrealizedPnlInPips = 5.5 },
                    },
                    TradeDescription = tradeDescription
                };

                var result = await connector.PostNewUnrealizedPnlSerie(serie);

                Assert.IsTrue(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
