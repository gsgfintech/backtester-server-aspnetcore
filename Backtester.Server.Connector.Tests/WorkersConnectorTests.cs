using Capital.GSG.FX.Data.Core.SystemData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class WorkersConnectorTests
    {
        [TestMethod]
        public async Task TestRequestNewJob()
        {
            string controllerEndpoint = "https://localhost:44379/";

            WorkersConnector connector = WorkersConnector.GetConnector(controllerEndpoint);

            string workerName = "RegTestWorker";

            var result = await connector.RequestNewJob(workerName);

            Assert.IsTrue(result.Success, result.Message);
        }

        [TestMethod]
        public async Task TestPostStatus()
        {
            string controllerEndpoint = "https://localhost:44379/";

            WorkersConnector connector = WorkersConnector.GetConnector(controllerEndpoint);

            SystemStatus status = new SystemStatus("RegtestEngine");
            status.Attributes.AddRange(new SystemStatusAttribute[] {
                new SystemStatusAttribute("IsAlive", true.ToString(), SystemStatusLevel.GREEN),
                new SystemStatusAttribute("IsConnectedToIB", false.ToString(), SystemStatusLevel.RED)
            });

            var result = await connector.PostStatus("regtestWorker", status);

            Assert.IsNotNull(result);
        }
    }
}
