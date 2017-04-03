using System;

namespace Backtester.Server.Controllers.Info
{
    public class InfoControllerUtils
    {
        public string AppName { get; private set; }
        public DateTimeOffset StartTime { get; private set; }

        public InfoControllerUtils(string appName, DateTimeOffset startTime)
        {
            AppName = appName;
            StartTime = startTime;
        }
    }
}
