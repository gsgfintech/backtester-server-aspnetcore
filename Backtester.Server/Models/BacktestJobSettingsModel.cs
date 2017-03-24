using Capital.GSG.FX.Data.Core.ContractData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backtester.Server.Models
{
    public class BacktestJobSettingsModel
    {
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

        [Display(Name = "Pairs")]
        public List<Cross> Crosses { get; set; }

        [Display(Name = "Begin Date")]
        [DisplayFormat(DataFormatString = "dd/MM/yy")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "dd/MM/yy")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Begin Time (HKT)")]
        [DisplayFormat(DataFormatString = "HH:mm")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time (HKT)")]
        [DisplayFormat(DataFormatString = "HH:mm")]
        public DateTime EndTime { get; set; }
    }
}