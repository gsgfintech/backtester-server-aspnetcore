using Microsoft.VisualStudio.TestTools.UnitTesting;
using Backtester.Server.Connector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Capital.GSG.FX.Backtest.DataTypes;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class JobsConnectorTests
    {
        [TestMethod]
        public async Task TestNotifyJobStarted()
        {
            string controllerEndpoint = "https://localhost:44379/";

            JobsConnector connector = JobsConnector.GetConnector(controllerEndpoint);

            string groupName = "RegTestJobGroup";
            string jobName = "RegTestJob";

            BacktestJob job = new BacktestJob(groupName, jobName)
            {
                ActualStartTime = DateTimeOffset.Now,
                CompletionTime = DateTimeOffset.Now,
                Day = DateTime.Today,
                EndTime = DateTimeOffset.Now,
                StartTime = DateTimeOffset.Now,
                Worker = "RegtestWorker"
            };

            var result = await connector.UpdateStatus(jobName, job);

            Assert.IsNotNull(result);
        }
    }
}
