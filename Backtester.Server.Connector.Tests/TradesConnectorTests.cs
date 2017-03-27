using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.ExecutionData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class ExecutionsConnectorTests
    {
        [TestMethod]
        public async Task TestPostNewTrade()
        {
            string controllerEndpoint = "https://localhost:44379/";

            TradesConnector connector = TradesConnector.GetConnector(controllerEndpoint);

            BacktestTrade trade = new BacktestTrade()
            {
                Size = 1000,
                TradeId = "2",
                Timestamp = DateTime.Now,
                OrderId = 2,
                Price = 1.5,
                Side = ExecutionSide.BOUGHT,
                Cross = Cross.EURUSD,
                CommissionUsd = 15.58,
                Duration = "01:50:00",
                RealizedPnL = 100,
                RealizedPnlPips = 10.0,
                RealizedPnlUsd = 100
            };

            var result = await connector.PostTrade("RegTestJob", trade);
            Assert.IsTrue(result.Success, result.Message);
        }
    }
}
