using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ViewModels.JobGroups
{
    public class JobGroupStatisticsViewModel
    {
        public string JobGroupId { get; private set; }

        [Display(Name = "Trades")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TradesCount => LongsCount + ShortsCount;

        [Display(Name = "Winners")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int WinnersCount => LongsWon + ShortsWon;

        [Display(Name = "Losers")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int LosersCount => TradesCount - WinnersCount;

        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public int Profitability => (TradesCount > 0) ? (int)((WinnersCount / (double)TradesCount) * 100) : 0;

        [Display(Name = "Pips")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public double TotalPips { get; set; }

        [Display(Name = "Average Win")]
        [DisplayFormat(DataFormatString = "{0:N1} pips")]
        public double AverageWinPips { get; set; }

        [Display(Name = "Average Win")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double AverageWinUsd { get; set; }

        [Display(Name = "Average Loss")]
        [DisplayFormat(DataFormatString = "{0:N1} pips")]
        public double AverageLossPips { get; set; }

        [Display(Name = "Average Loss")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double AverageLossUsd { get; set; }

        [Display(Name = "Volume")]
        [DisplayFormat(DataFormatString = "{0:N0} USD")]
        public double TotalVolume { get; set; }

        [Display(Name = "Commissions")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public int TotalFees { get; set; }

        [Display(Name = "Longs Count")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int LongsCount { get; set; }

        [Display(Name = "Longs Won Count")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int LongsWon { get; set; }

        [Display(Name = "Longs Won")]
        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double LongsWonRatio => (LongsCount > 0) ? (LongsWon / (double)LongsCount) * 100 : 0;

        [Display(Name = "Shorts Count")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ShortsCount { get; set; }

        [Display(Name = "Shorts Won Count")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ShortsWon { get; set; }

        [Display(Name = "Shorts Won")]
        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public double ShortsWonRatio => (ShortsCount > 0) ? (ShortsWon / (double)ShortsCount) * 100 : 0;

        [Display(Name = "Best Trade (USD)")]
        public BacktestTradeModel BestTradeUsd { get; set; }

        [Display(Name = "Worst Trade (USD)")]
        public BacktestTradeModel WorstTradeUsd { get; set; }

        [Display(Name = "Best Trade (pips)")]
        public BacktestTradeModel BestTradePips { get; set; }

        [Display(Name = "Worst Trade (pips)")]
        public BacktestTradeModel WorstTradePips { get; set; }

        [Display(Name = "Average Trade Duration")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}")]
        public TimeSpan AverageTradeDuration { get; set; }

        [Display(Name = "Profit Factor")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double ProfitFactor { get; set; }

        [Display(Name = "Standard Deviation")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double StandardDeviationUsd { get; set; }

        [Display(Name = "Standard Deviation")]
        [DisplayFormat(DataFormatString = "{0:N1} pips")]
        public double StandardDeviationPips { get; set; }

        [Display(Name = "Sharpe Ratio")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double SharpeRatio { get; set; }

        [Display(Name = "Expectancy")]
        [DisplayFormat(DataFormatString = "{0:N2} pips")]
        public double ExpectancyPips { get; set; }

        [Display(Name = "Expectancy")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double ExpectancyUsd { get; set; }

        [Display(Name = "Max Drawdown")]
        [DisplayFormat(DataFormatString = "{0:N2}%")]
        public double MaxDrawdown { get; set; }

        [Display(Name = "Max Drawdown Duration")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}")]
        public TimeSpan MaxDrawdownDuration { get; set; }

        public List<JobGroupPerCrossStatisticsViewModel> PerCrossStatistics { get; set; }

        public JobGroupStatisticsViewModel(string jobGroupId)
        {
            JobGroupId = jobGroupId;
        }
    }

    public class JobGroupPerCrossStatisticsViewModel
    {
        public Cross Pair { get; set; }

        [Display(Name = "Trades")]
        public int TradesCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0} USD")]
        public double Volume { get; set; }

        [Display(Name = "Gross")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double TotalGrossUsd { get; set; }

        [Display(Name = "Commissions")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double TotalFeesUsd { get; set; }

        [Display(Name = "Net")]
        [DisplayFormat(DataFormatString = "{0:N2} USD")]
        public double TotalNet => TotalGrossUsd - TotalFeesUsd;

        [Display(Name = "Pips")]
        [DisplayFormat(DataFormatString = "{0:N1} pips")]
        public double TotalPips { get; set; }
    }
}
