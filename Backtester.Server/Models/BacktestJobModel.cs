﻿using Capital.GSG.FX.Backtest.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backtester.Server.Models
{
    public class BacktestJobModel
    {
        public string Name { get; set; }
        public string GroupId { get; set; }
        public BacktestJobStatusModel Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        public DateTime Day { get; set; }

        [Display(Name = "Test Start Time (UTC)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset StartTime { get; set; }

        [Display(Name = "Test End Time (UTC)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTimeOffset EndTime { get; set; }

        public BacktestJobOutputDataModel Output { get; set; }

        [Display(Name = "Used Historical Market Data")]
        public bool UsedHistoMarketData { get; set; }
    }

    public class BacktestJobOutputDataModel
    {
        public List<BacktestTradeModel> Trades { get; set; }
        public List<BacktestOrderModel> Orders { get; set; }
        public List<BacktestPositionModel> Positions { get; set; }
        public List<AlertModel> Alerts { get; set; }
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
                Trades = output.Trades.Values.ToTradeModels()
            };
        }

        public static BacktestJobModel ToBacktestJobModel(this BacktestJob job)
        {
            if (job == null)
                return null;

            return new BacktestJobModel()
            {
                CreateTime = job.CreateTime,
                Day = job.Day,
                EndTime = job.EndTime,
                GroupId = job.GroupId,
                Name = job.Name,
                Output = job.Output.ToBacktestJobOutputDataModel(),
                StartTime = job.StartTime,
                Status = job.Status.ToBacktestStatusModel(),
                UsedHistoMarketData = job.UsedHistoData
            };
        }
    }
}
