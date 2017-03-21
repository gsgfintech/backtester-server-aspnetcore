using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestStatusModel
    {
        public List<BacktestStatusAttributeModel> Attributes { get; set; }
        public string Message { get; set; }
        public double Progress { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class BacktestStatusAttributeModel
    {
        public string Name { get; set; }
        public object Value { get; set; }
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
                Value = attribute.Value
            };
        }

        private static List<BacktestStatusAttributeModel> ToBacktestStatusAttributeModels(this IEnumerable<BacktestStatusAttribute> attributes)
        {
            return attributes?.Select(a => a.ToBacktestStatusAttributeModel()).ToList();
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
                Timestamp = status.Timestamp
            };
        }
    }
}
