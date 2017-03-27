using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class PositionsConnectorTests
    {
        [TestMethod]
        public async Task TestPostNewPosition()
        {
            string controllerEndpoint = "https://localhost:44379/";

            PositionsConnector connector = PositionsConnector.GetConnector(controllerEndpoint);

            BacktestPosition position = new BacktestPosition()
            {
                LastUpdate = DateTimeOffset.Now,
                Cross = Cross.EURUSD,
                PositionQuantity = 10000,
                RealizedPnL = 100,
                RealizedPnlUsd = 100,
                UnrealizedPnL = 100,
                UnrealizedPnlUsd = 100
            };

            var result = await connector.PostPosition("RegTestJob", position);
            Assert.IsTrue(result.Success, result.Message);
        }
    }
}
