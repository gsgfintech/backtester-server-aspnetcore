using Capital.GSG.FX.Data.Core.SystemData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class AlertsConnectorTests
    {
        [TestMethod]
        public async Task TestPostNewAlert()
        {
            string monitoringEndpoint = "https://localhost:44379/";

            AlertsConnector connector = AlertsConnector.GetConnector(monitoringEndpoint);

            try
            {
                Alert alert = new Alert()
                {
                    AlertId = Guid.NewGuid().ToString(),
                    Body = "This is a test",
                    Level = AlertLevel.INFO,
                    Source = "UnitTest",
                    Status = AlertStatus.OPEN,
                    Subject = "Test",
                    Timestamp = DateTimeOffset.Now
                };

                var result = await connector.PostNewAlert("regtestJob", alert);
                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
