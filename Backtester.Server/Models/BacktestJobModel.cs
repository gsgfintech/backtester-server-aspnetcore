using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backtester.Server.Models
{
    public class BacktestJobModel
    {
        public string Name { get; set; }
        public string GroupId { get; set; }
        public BacktestJobStatus Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset? ActualStartTime { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }
        public string Worker { get; set; }
        public DateTime Day { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public BacktestJobOutputDataModel Output { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double Progress { get; set; }
    }

    public class BacktestJobOutputDataModel
    {
        public List<BacktestTradeModel> Trades { get; set; }
        public List<BacktestOrderModel> Orders { get; set; }
        public List<BacktestPositionModel> Positions { get; set; }
        public List<AlertModel> Alerts { get; set; }
        public BacktestStatusModel Status { get; set; }
    }

    public static class BacktestJobModelExtensions
    {
        private static BacktestJobOutputDataModel ToBacktestJobOutputDataModel(this BacktestJobOutputData output)
        {
            if (output == null)
                return null;

            return new BacktestJobOutputDataModel()
            {
                Alerts = output.Alerts.ToAlertModels(),
                Orders = output.Orders.ToOrderModels(),
                Positions = output.Positions.ToPositionModels(),
                Status = output.Status.ToBacktestStatusModel(),
                Trades = output.Trades.Values.ToTradeModels()
            };
        }

        public static BacktestJobModel ToBacktestJobModel(this BacktestJob job)
        {
            if (job == null)
                return null;

            return new BacktestJobModel()
            {
                ActualStartTime = job.ActualStartTime,
                CompletionTime = job.CompletionTime,
                CreateTime = job.CreateTime,
                Day = job.Day,
                EndTime = job.EndTime,
                GroupId = job.GroupId,
                Name = job.Name,
                Output = job.Output.ToBacktestJobOutputDataModel(),
                Progress = job.Progress,
                StartTime = job.StartTime,
                Status = job.Status,
                Worker = job.Worker
            };
        }
    }
}
