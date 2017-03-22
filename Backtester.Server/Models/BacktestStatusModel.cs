using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestStatusModel
    {
        public List<BacktestStatusAttributeModel> Attributes { get; set; }
        public string Message { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double Progress { get; set; }

        [Display(Name = "Current Time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss} (HKT)")]
        public DateTimeOffset Timestamp { get; set; }
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
            return attributes?.Select(a => a.ToBacktestStatusAttributeModel()).OrderBy(a => a.Name).ToList();
        }

        public static BacktestStatusModel ToBacktestStatusModel(this BacktestStatus status)
        {
            if (status == null)
                return null;

            return new BacktestStatusModel()
            {
                Attributes = status.Attributes.ToBacktestStatusAttributeModels(),
                Message = status.Message,
                Progress = status.Progress,
                Timestamp = status.Timestamp.ToLocalTime()
            };
        }
    }
}
