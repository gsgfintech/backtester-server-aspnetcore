using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Utils.Core;
using DataTypes.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestJobGroupModel
    {

        [Display(Name = "ID")]
        public string GroupId { get; set; }

        public Dictionary<string, string> JobIds { get; set; }

        public BacktestJobStatus Status { get; set; }

        [Display(Name = "Time Created (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset CreateTime { get; set; }

        [Display(Name = "Scheduled Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset? ScheduledStartTime { get; set; }

        [Display(Name = "Actual Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset? ActualStartTime { get; set; }

        [Display(Name = "Time Completed (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset? CompletionTime { get; set; }

        public TimeSpan? Duration => (ActualStartTime.HasValue && CompletionTime.HasValue) ? CompletionTime.Value.Subtract(ActualStartTime.Value) : (TimeSpan?)null;

        [Display(Name = "Test Start Date (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTimeOffset StartDate { get; set; }

        [Display(Name = "Test End Date (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTimeOffset EndDate { get; set; }

        [Display(Name = "Test Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset StartTime { get; set; }

        [Display(Name = "Test End Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset EndTime { get; set; }

        public BacktestJobStrategyModel Strategy { get; set; }

        public List<BacktestTradeModel> Trades { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}%")]
        public double? Progress { get; set; }
    }

    public class BacktestJobStrategyModel
    {
        public string Crosses { get; set; }

        public string Name { get; set; }

        public List<BacktestJobStrategyParameterModel> Parameters { get; set; }

        [Display(Name = "DLL Path")]
        public string DllPath { get; set; }

        public string Version { get; set; }

        [Display(Name = "Strategy Type Name")]
        public string StrategyTypeName { get; set; }

        [Display(Name = "Algo Type Name")]
        public string AlgoTypeName { get; set; }
    }

    public class BacktestJobStrategyParameterModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Tooltip { get; set; }
    }

    public static class BacktestJobGroupModelExtensions
    {
        private static BacktestJobStrategyParameterModel ToBacktestJobStrategyParameterModel(this StrategyParameter parameter)
        {
            if (parameter == null)
                return null;

            return new BacktestJobStrategyParameterModel()
            {
                Name = parameter.Name,
                Tooltip = parameter.Tooltip,
                Type = parameter.Type,
                Value = parameter.Value
            };
        }

        private static List<BacktestJobStrategyParameterModel> ToBacktestJobStrategyParameterModels(this IEnumerable<StrategyParameter> parameters)
        {
            return parameters?.Select(p => p.ToBacktestJobStrategyParameterModel()).ToList();
        }

        private static BacktestJobStrategyModel ToBacktestJobStrategyModel(this Strategy strategy)
        {
            if (strategy == null)
                return null;

            return new BacktestJobStrategyModel()
            {
                AlgoTypeName = strategy.AlgoTypeName,
                Crosses = !strategy.Crosses.IsNullOrEmpty() ? string.Join(", ", strategy.Crosses) : string.Empty,
                Name = strategy.Name,
                Parameters = strategy.Parameters.ToBacktestJobStrategyParameterModels(),
                DllPath = strategy.StrategyDllPath,
                Version = strategy.Version,
                StrategyTypeName = strategy.StrategyTypeName
            };
        }

        public static BacktestJobGroupModel ToBacktestJobGroupModel(this BacktestJobGroup group)
        {
            if (group == null)
                return null;

            return new BacktestJobGroupModel()
            {
                ActualStartTime = group.ActualStartTime,
                CompletionTime = group.CompletionTime,
                CreateTime = group.CreateTime,
                EndDate = group.EndDate,
                EndTime = group.EndTime,
                GroupId = group.GroupId,
                JobIds = group.JobIds,
                Progress = 56.8, // TODO
                ScheduledStartTime = group.ScheduledStartTime,
                StartDate = group.StartDate,
                StartTime = group.StartTime,
                Status = group.Status,
                Strategy = group.Strategy.ToBacktestJobStrategyModel(),
                Trades = group.Trades.ToTradeModels()
            };
        }

        public static List<BacktestJobGroupModel> ToBacktestJobGroupModels(this IEnumerable<BacktestJobGroup> groups)
        {
            return groups?.Select(g => g.ToBacktestJobGroupModel()).ToList();
        }
    }
}
