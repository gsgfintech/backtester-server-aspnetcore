using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
                Day = DateTime.Today,
                EndTime = DateTimeOffset.Now,
                StartTime = DateTimeOffset.Now
            };

            job.Status.ActualStartTime = DateTimeOffset.Now;
            job.Status.CompletionTime = DateTimeOffset.Now;
            job.Status.Worker = "RegtestWorker";

            var result = await connector.UpdateStatus(jobName, job);

            Assert.IsNotNull(result);
        }
    }
}
