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

        public Dictionary<string, BacktestJobLightModel> Jobs { get; set; }

        public BacktestJobStatusCode Status { get; set; }

        [Display(Name = "Time Created (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset CreateTime { get; set; }

        [Display(Name = "Actual Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset? ActualStartTime { get; set; }

        [Display(Name = "Time Completed (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset? CompletionTime { get; set; }

        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}")]
        public TimeSpan? Duration => (ActualStartTime.HasValue && CompletionTime.HasValue) ? CompletionTime.Value.Subtract(ActualStartTime.Value) : (TimeSpan?)null;

        [Display(Name = "Test Start Date (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTimeOffset StartDate { get; set; }

        [Display(Name = "Test End Date (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTimeOffset EndDate { get; set; }

        [Display(Name = "Time Frame")]
        public string TimeFrame => $"{StartDate:dd/MM/yy} - {EndDate:dd/MM/yy}";

        [Display(Name = "Test Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset StartTime { get; set; }

        [Display(Name = "Test End Time (EST/EDT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset EndTime { get; set; }

        public BacktestJobStrategyModel Strategy { get; set; }

        public List<BacktestTradeModel> Trades { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double Progress { get; set; }

        [Display(Name = "Use historical database if no data is available in the IB database")]
        public bool UseHistoDatabase { get; set; }

        [Display(Name = "PnL (USD)")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double? NetRealizedPnlUsd => !Jobs.IsNullOrEmpty() ? Jobs.Select(j => j.Value.NetRealizedPnlUsd ?? 0).Sum() : (double?)null;
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
        public string Tooltip { get; set; }
    }

    public static class BacktestJobGroupModelExtensions
    {
        private static BacktestJobStrategyParameterModel ToBacktestJobStrategyParameterModel(this StrategyParameter parameter, string namePrefix)
        {
            if (parameter == null)
                return null;

            return new BacktestJobStrategyParameterModel()
            {
                Name = !string.IsNullOrEmpty(namePrefix) ? parameter.Name.Replace(namePrefix, "") : parameter.Name,
                Tooltip = parameter.Tooltip,
                Value = parameter.Value
            };
        }

        public static List<BacktestJobStrategyParameterModel> ToBacktestJobStrategyParameterModels(this IEnumerable<StrategyParameter> parameters, string namePrefix = "")
        {
            return parameters?.Select(p => p.ToBacktestJobStrategyParameterModel(namePrefix)).ToList();
        }

        private static BacktestJobStrategyModel ToBacktestJobStrategyModel(this Strategy strategy)
        {
            if (strategy == null)
                return null;

            return new BacktestJobStrategyModel()
            {
                AlgoTypeName = strategy.AlgoTypeName,
                Crosses = !strategy.CrossesAndTicketSizes.IsNullOrEmpty() ? string.Join(", ", strategy.CrossesAndTicketSizes.Select(c => $"{c.Key} ({c.Value / 1000:N0}K)")) : string.Empty,
                Name = strategy.Name,
                Parameters = strategy.Parameters.ToBacktestJobStrategyParameterModels("Param"),
                DllPath = strategy.StrategyDllPath,
                Version = strategy.Version,
                StrategyTypeName = strategy.StrategyTypeName
            };
        }

        public static BacktestJobGroupModel ToBacktestJobGroupModel(this BacktestJobGroup group)
        {
            if (group == null)
                return null;

            DateTimeOffset? actualStartTime = !group.Jobs.IsNullOrEmpty() ? group.GetActualStartTime() : null;
            DateTimeOffset? completionTime = !group.Jobs.IsNullOrEmpty() ? group.GetCompletionTime() : null;

            return new BacktestJobGroupModel()
            {
                ActualStartTime = actualStartTime.HasValue ? actualStartTime.Value.ToLocalTime() : (DateTimeOffset?)null,
                CompletionTime = completionTime.HasValue ? completionTime.Value.ToLocalTime() : (DateTimeOffset?)null,
                CreateTime = group.CreateTime.ToLocalTime(),
                EndDate = group.EndDate.ToLocalTime(),
                EndTime = group.EndTime.ToLocalTime(),
                GroupId = group.GroupId,
                Jobs = group.Jobs.ToBacktestJobLightModels(),
                Progress = !group.Jobs.IsNullOrEmpty() ? group.GetProgress() : 0,
                StartDate = group.StartDate.ToLocalTime(),
                StartTime = group.StartTime.ToLocalTime(),
                Status = !group.Jobs.IsNullOrEmpty() ? group.GetStatus() : BacktestJobStatusCode.UNKNOWN,
                Strategy = group.Strategy.ToBacktestJobStrategyModel(),
                Trades = group.Trades.ToTradeModels(),
                UseHistoDatabase = group.UseHistoDatabase
            };
        }

        public static List<BacktestJobGroupModel> ToBacktestJobGroupModels(this IEnumerable<BacktestJobGroup> groups)
        {
            return groups?.Select(g => g.ToBacktestJobGroupModel()).ToList() ?? new List<BacktestJobGroupModel>();
        }

        private static StrategyParameter ToStrategyParameter(this BacktestJobStrategyParameterModel parameter, string namePrefix)
        {
            if (parameter == null)
                return null;

            return new StrategyParameter()
            {
                Name = (!string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(namePrefix) && !parameter.Name.StartsWith(namePrefix)) ? $"{namePrefix}{parameter.Name}" : parameter.Name,
                Tooltip = parameter.Tooltip,
                Value = parameter.Value
            };
        }

        public static List<StrategyParameter> ToStrategyParameters(this IEnumerable<BacktestJobStrategyParameterModel> parameters, string namePrefix = "")
        {
            return parameters?.Select(p => p.ToStrategyParameter(namePrefix)).ToList();
        }
    }
}
