using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestJobLightModel
    {
        public DateTimeOffset? ActualStartTime { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }
        public string DayStr { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double Progress { get; set; }

        public BacktestJobStatusCode StatusCode { get; set; }

        [Display(Name = "Used Historical Market Data")]
        public bool UsedHistoMarketData { get; set; }

        public string Worker { get; set; }
    }

    public static class BacktestJobLightModelExtensions
    {
        private static BacktestJobLightModel ToBacktestJobLightModel(this BacktestJobLight job)
        {
            if (job == null)
                return null;

            return new BacktestJobLightModel()
            {
                ActualStartTime = job.ActualStartTime,
                CompletionTime = job.CompletionTime,
                DayStr = job.DayStr,
                Progress = job.Progress,
                StatusCode = job.StatusCode,
                UsedHistoMarketData = job.UsedHistoData,
                Worker = job.Worker
            };
        }

        public static Dictionary<string, BacktestJobLightModel> ToBacktestJobLightModels(this Dictionary<string, BacktestJobLight> jobs)
        {
            return jobs?.ToDictionary(j => j.Key, j => j.Value.ToBacktestJobLightModel());
        }
    }
}
