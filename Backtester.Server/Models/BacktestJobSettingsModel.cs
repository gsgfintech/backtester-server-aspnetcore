﻿using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Utils.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktestJobSettingsModel
    {
        [Display(Name = "Job")]
        public string JobName { get; set; }

        [Display(Name = "Original File Name")]
        public string OriginalFileName { get; set; }

        [Display(Name = "New File Name")]
        public string NewFileName { get; set; }

        public List<BacktestJobStrategyParameterModel> Parameters { get; set; }

        [Display(Name = "Strat Name")]
        public string StrategyName { get; set; }

        [Display(Name = "Strat Version")]
        public string StrategyVersion { get; set; }

        [Display(Name = "Strat Class")]
        public string StrategyClass { get; set; }

        [Display(Name = "Algo Class")]
        public string AlgorithmClass { get; set; }

        public Dictionary<Cross, int> CrossesAndTicketSizes { get; set; }

        [Display(Name = "Pairs")]
        public string CrossesStr => !CrossesAndTicketSizes.IsNullOrEmpty() ? string.Join(", ", CrossesAndTicketSizes.Select(c => $"{c.Key} ({c.Value / 1000:N0}K)")) : string.Empty;

        [Display(Name = "Begin Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Business Days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalDays => DateTimeUtils.EachBusinessDay(StartDate, EndDate).Count();

        [Display(Name = "Begin Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time (EST/EDT)")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Use historical database if no data is available in the IB database")]
        public bool UseHistoDatabase { get; set; }
    }
}