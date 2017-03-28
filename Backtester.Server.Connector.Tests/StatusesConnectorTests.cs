using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.SystemData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class StatusesConnectorTests
    {
        [TestMethod]
        public async Task TestPostStatusUpdate()
        {
            string controllerEndpoint = "https://localhost:44379/";

            StatusesConnector connector = StatusesConnector.GetConnector(controllerEndpoint);

            try
            {
                BacktestStatus status = new BacktestStatus()
                {
                    Attributes = new List<BacktestStatusAttribute>()
                    {
                        new BacktestStatusAttribute() { Name = "Attribute1", Value = "Value1" },
                        new BacktestStatusAttribute() { Name = "Attribute2", Value = "Value2" }
                    },
                    Message = "Test Message",
                    Progress = 50,
                    Timestamp = DateTimeOffset.Now
                };

                var result = await connector.PostBacktestStatus("RegTestJob", status);

                Assert.IsTrue(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
