using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestJobStatusModel
    {
        public List<BacktestStatusAttributeModel> Attributes { get; set; }
        public string Message { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double Progress { get; set; }

        [Display(Name = "Current Time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss} (HKT)")]
        public DateTimeOffset? Timestamp { get; set; }

        public DateTimeOffset? ActualStartTime { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }

        public BacktestJobStatusCode StatusCode { get; set; }

        public string Worker { get; set; }

        [Display(Name = "Used Historical Market Data")]
        public bool UsedHistoMarketData { get; set; }
    }

    public class BacktestStatusAttributeModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    static class BacktestStatusModelExtensions
    {
        private static BacktestStatusAttributeModel ToBacktestStatusAttributeModel(this BacktestStatusAttribute attribute)
        {
            if (attribute == null)
                return null;

            return new BacktestStatusAttributeModel()
            {
                Name = attribute.Name,
                Value = attribute.Value.ToString()
            };
        }

        private static List<BacktestStatusAttributeModel> ToBacktestStatusAttributeModels(this IEnumerable<BacktestStatusAttribute> attributes)
        {
            return attributes?.Select(a => a.ToBacktestStatusAttributeModel()).Where(a => a != null)?.OrderBy(a => a.Name).ToList();
        }

        public static BacktestJobStatusModel ToBacktestStatusModel(this BacktestJobStatus status, bool usedHistoMd)
        {
            if (status == null)
                return null;

            return new BacktestJobStatusModel()
            {
                ActualStartTime = status.ActualStartTime,
                Attributes = status.Attributes.ToBacktestStatusAttributeModels(),
                CompletionTime = status.CompletionTime,
                Message = status.Message,
                Progress = status.Progress,
                StatusCode = status.StatusCode,
                Timestamp = (status.Timestamp > DateTimeOffset.MinValue) ? status.Timestamp.ToLocalTime() : (DateTimeOffset?)null,
                UsedHistoMarketData = usedHistoMd,
                Worker = status.Worker
            };
        }
    }
}
