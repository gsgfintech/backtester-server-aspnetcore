﻿using Backtester.Server.BatchWorker.Connector;
using Backtester.Server.Models;
using Backtester.Server.ViewModels.JobGroups;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Data.Core.ExecutionData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using DataTypes.Core;
using MathNet.Numerics.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class JobGroupsControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<JobGroupsControllerUtils>();

        private readonly BacktestJobGroupActioner actioner;
        private readonly JobsControllerUtils jobsControllerUtils;

        private readonly AllTradesConnector allTradesConnector;
        private readonly TradeGenericMetric2SeriesConnector tradeGenericMetric2SeriesConnector;
        private readonly UnrealizedPnlSeriesConnector unrealizedPnlSeriesConnector;

        private List<string> stratsNames = null;

        private ConcurrentDictionary<string, List<BacktestJobGroup>> searchResults = new ConcurrentDictionary<string, List<BacktestJobGroup>>();

        private ConcurrentDictionary<string, DateTimeOffset> jobGroupsWithFilesRequested = new ConcurrentDictionary<string, DateTimeOffset>();

        private ConcurrentQueue<string> cachedJobsList = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, BacktestJobGroup> cachedJobs = new ConcurrentDictionary<string, BacktestJobGroup>();

        private bool resetPendingJobsRequested = false;
        private object resetPendingJobsRequestedLocker = new object();

        public JobGroupsControllerUtils(BacktestJobGroupActioner actioner, JobsControllerUtils jobsControllerUtils, AllTradesConnector allTradesConnector, TradeGenericMetric2SeriesConnector tradeGenericMetric2SeriesConnector, UnrealizedPnlSeriesConnector unrealizedPnlSeriesConnector)
        {
            this.actioner = actioner;
            this.jobsControllerUtils = jobsControllerUtils;

            this.allTradesConnector = allTradesConnector;
            this.tradeGenericMetric2SeriesConnector = tradeGenericMetric2SeriesConnector;
            this.unrealizedPnlSeriesConnector = unrealizedPnlSeriesConnector;
        }

        internal async Task<List<BacktestJobGroup>> GetActiveJobs()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            var jobGroups = await actioner.GetAllActive(cts.Token);

            if (!jobGroups.IsNullOrEmpty())
            {
                jobGroups.Sort(BacktestJobGroupCreationDateComparer.Instance);

                if (!resetPendingJobsRequested)
                {
                    lock (resetPendingJobsRequestedLocker)
                    {
                        resetPendingJobsRequested = true;
                    }

                    logger.Info($"First time loading active jobs: requesting {nameof(jobsControllerUtils)} to reset pending jobs");

                    // Retrieve all subjobs that are not already completed
                    var pendingJobs = jobGroups.Select(g => g.Jobs.Where(j => j.Value.StatusCode != BacktestJobStatusCode.COMPLETED).Select(sg => sg.Key).ToList()).Aggregate((cur, next) => cur.Concat(next).ToList());

                    jobsControllerUtils.ResetPendingJobs(pendingJobs);
                }
            }

            return jobGroups;
        }

        internal async Task<List<BacktestJobGroup>> GetInactiveJobs()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            var jobGroups = await actioner.GetTodaysInactive(cts.Token);

            if (!jobGroups.IsNullOrEmpty())
                jobGroups.Sort(BacktestJobGroupCreationDateComparer.Instance);

            return jobGroups;
        }

        internal async Task<BacktestJobGroup> Get(string groupId)
        {
            if (cachedJobs.TryGetValue(groupId, out BacktestJobGroup jobGroup))
                return jobGroup;
            else
            {
                logger.Info($"Querying backtest job group {groupId} from database");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(5));

                jobGroup = await actioner.Get(groupId, cts.Token);

                if (jobGroup != null && !jobGroup.Jobs.IsNullOrEmpty() && BacktestJobStatusCodeUtils.InactiveStatus.Contains(jobGroup.GetStatus()) && jobGroup.Trades.IsNullOrEmpty() && !jobGroupsWithFilesRequested.ContainsKey(groupId))
                {
                    if (jobGroupsWithFilesRequested.TryAdd(groupId, DateTimeOffset.Now))
                    {
                        logger.Info($"Will request to compute trade list and generate Excel files for newly inactive job group {groupId} and update in database");

                        await allTradesConnector.PostFileRequest(groupId);

                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();

                        await tradeGenericMetric2SeriesConnector.PostFileRequest(groupId);

                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();

                        await unrealizedPnlSeriesConnector.PostUnrealizedPnlFileRequest(groupId);

                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();

                        await unrealizedPnlSeriesConnector.PostUnrealizedPnlPerHourFileRequest(groupId);
                    }
                }

                return jobGroup;
            }
        }

        internal async Task<List<BacktestTradeModel>> GetTrades(string groupId)
        {
            var jobGroup = await Get(groupId);

            return jobGroup?.Trades.ToTradeModels();
        }

        internal async Task<GenericActionResult> AddJobGroup(BacktestJobGroup group)
        {
            logger.Info($"Adding job group {group.GroupId} to database");

            return await actioner.AddOrUpdate(group.GroupId, group);
        }

        private async Task<List<BacktestTrade>> ComputeTradesList(IEnumerable<string> jobIds)
        {
            if (jobIds.IsNullOrEmpty())
                return new List<BacktestTrade>();

            var jobs = await jobsControllerUtils.GetMany(jobIds);

            if (jobs == null)
                return new List<BacktestTrade>();

            return jobs.Select(j => j.Output.Trades.Values.ToList()).Aggregate((cur, next) => cur.Concat(next).ToList());
        }

        internal async Task<GenericActionResult> Delete(string groupId)
        {
            logger.Info($"About to delete job group {groupId} and related subjobs");

            var jobGroup = await Get(groupId);

            if (jobGroup == null)
                return new GenericActionResult(false, $"Unable to delete unknown job group {groupId}");

            if (!jobGroup.Jobs.IsNullOrEmpty())
            {
                var deleteSubjobs = await jobsControllerUtils.DeleteMany(jobGroup.Jobs.Keys);

                if (!deleteSubjobs.Success)
                    return new GenericActionResult(false, $"Failed to delete one or more subjob ({string.Join(", ", jobGroup.Jobs)}) for job group {groupId}: {deleteSubjobs.Message}. Not deleting subgroup");
            }

            return await actioner.Delete(groupId);
        }

        internal async Task<List<string>> ListStratsNames()
        {
            if (stratsNames == null)
                stratsNames = (await actioner.GetAllStratNames()) ?? new List<string>();

            return stratsNames;
        }

        internal async Task<string> Search(string groupId, string stratName, DateTimeOffset? rangeStart, DateTimeOffset? rangeEnd)
        {
            var results = await actioner.Search(groupId, stratName, null, rangeStart, rangeEnd);

            string searchId = Guid.NewGuid().ToString();

            searchResults.TryAdd(searchId, results);

            return searchId;
        }

        internal List<BacktestJobGroup> GetSearchResults(string searchId)
        {
            List<BacktestJobGroup> results;

            if (searchResults.TryRemove(searchId, out results))
                return results;
            else
                return new List<BacktestJobGroup>();
        }

        internal async Task<JobGroupStatisticsViewModel> ComputeStatistics(string groupId)
        {
            double averageLossPips = 0;
            double averageLossUsd = 0;
            TimeSpan averageTradeDuration = new TimeSpan(0, 0, 0);
            double averageWinPips = 0;
            double averageWinUsd = 0;
            BacktestTradeModel bestTradePips = null;
            BacktestTradeModel bestTradeUsd = null;
            double expectancyPips = 0;
            double expectancyUsd = 0;
            int longsCount = 0;
            int longsWon = 0;
            double maxDrawdown = 0; //
            TimeSpan maxDrawdownDuration = new TimeSpan(0, 0, 0); //
            double profitFactor = 0;
            double sharpeRatio = 0;
            int shortsCount = 0;
            int shortsWon = 0;
            double standardDeviationUsd = 0;
            double standardDeviationPips = 0;
            int totalFees = 0;
            double totalPips = 0;
            double totalVolume = 0; // TODO : need to add SizeUSD property on the trade object
            BacktestTradeModel worstTradePips = null;
            BacktestTradeModel worstTradeUsd = null;
            List<JobGroupPerCrossStatisticsViewModel> perCrossStats;

            var trades = await GetTrades(groupId);

            if (!trades.IsNullOrEmpty())
            {
                double grossLoss = 0;
                double grossProfit = 0;

                #region Losers
                var losers = trades.Where(t => t.RealizedPnlUsd < 0);

                if (!losers.IsNullOrEmpty())
                {
                    averageLossPips = losers.Select(t => t.RealizedPnlPips.Value).Average();
                    averageLossUsd = losers.Select(t => t.RealizedPnlUsd.Value).Average();

                    double maxLossPips = losers.Select(t => t.RealizedPnlPips.Value).Min();
                    worstTradePips = losers.FirstOrDefault(t => t.RealizedPnlPips == maxLossPips);

                    double maxLossUsd = losers.Select(t => t.RealizedPnlUsd.Value).Min();
                    worstTradeUsd = losers.FirstOrDefault(t => t.RealizedPnlUsd == maxLossUsd);

                    grossLoss = losers.Select(t => t.RealizedPnlPips.Value).Sum();
                }
                #endregion

                #region Winners
                var winners = trades.Where(t => t.RealizedPnlUsd > 0);

                if (!winners.IsNullOrEmpty())
                {
                    averageWinPips = winners.Select(t => t.RealizedPnlPips.Value).Average();
                    averageWinUsd = winners.Select(t => t.RealizedPnlUsd.Value).Average();

                    double maxWinPips = winners.Select(t => t.RealizedPnlPips.Value).Max();
                    bestTradePips = winners.FirstOrDefault(t => t.RealizedPnlPips == maxWinPips);

                    double maxWinUsd = winners.Select(t => t.RealizedPnlUsd.Value).Max();
                    bestTradeUsd = winners.FirstOrDefault(t => t.RealizedPnlUsd == maxWinUsd);

                    grossProfit = winners.Select(t => t.RealizedPnlPips.Value).Sum();
                }
                #endregion

                #region Longs
                var longs = trades.Where(t => t.IsPositionClosing() && t.Side == ExecutionSide.SOLD); // position closing trade is a SELL (ie the position was LONG)

                if (!longs.IsNullOrEmpty())
                {
                    longsCount = longs.Count();
                    longsWon = longs.Count(t => t.RealizedPnlUsd > 0);
                }
                #endregion

                #region Shorts
                var shorts = trades.Where(t => t.IsPositionClosing() && t.Side == ExecutionSide.BOUGHT); // position closing trade is a BUY (ie the position was SHORT)

                if (!shorts.IsNullOrEmpty())
                {
                    shortsCount = shorts.Count();
                    shortsWon = shorts.Count(t => t.RealizedPnlUsd > 0);
                }
                #endregion

                #region Trades With Duration
                var tradesWithDuration = trades.Where(t => t.Duration.HasValue);

                if (!tradesWithDuration.IsNullOrEmpty())
                {
                    long averageTradeDurationTicks = (long)tradesWithDuration.Select(t => t.Duration.Value.Ticks).Average();
                    averageTradeDuration = new TimeSpan(averageTradeDurationTicks);
                }
                #endregion

                #region Trades With PnL
                var tradesWithPnl = losers.Concat(winners);

                if (!tradesWithPnl.IsNullOrEmpty())
                {
                    standardDeviationUsd = tradesWithPnl.Select(t => t.RealizedPnlUsd.Value).StandardDeviation();
                    standardDeviationPips = tradesWithPnl.Select(t => t.RealizedPnlPips.Value).StandardDeviation();

                    double averageReturnUsd = tradesWithPnl.Select(t => t.RealizedPnlUsd.Value).Average();

                    if (averageReturnUsd != 0 && standardDeviationUsd != 0)
                        sharpeRatio = averageReturnUsd / standardDeviationUsd;
                }
                #endregion

                #region All
                totalFees = (int)trades.Select(t => t.CommissionUsd ?? 0).Sum();
                totalPips = trades.Select(t => t.RealizedPnlPips ?? 0).Sum();
                totalVolume = trades.Select(t => t.SizeUsd ?? 0).Sum() * 1000; // Sizes are divided by 1000 when the BacktestTradeModel is created

                double totalUsd = trades.Select(t => t.RealizedPnlUsd ?? 0).Sum();

                expectancyPips = totalPips / (longsCount + shortsCount);
                expectancyUsd = totalUsd / (longsCount + shortsCount);

                if (grossLoss != 0 && grossProfit != 0)
                    profitFactor = Math.Abs(grossProfit) / Math.Abs(grossLoss);
                #endregion

                perCrossStats = ComputePerCrossStats(trades);
            }
            else
                perCrossStats = new List<JobGroupPerCrossStatisticsViewModel>();

            return new JobGroupStatisticsViewModel(groupId)
            {
                AverageLossPips = averageLossPips,
                AverageLossUsd = averageLossUsd,
                AverageTradeDuration = averageTradeDuration,
                AverageWinPips = averageWinPips,
                AverageWinUsd = averageWinUsd,
                BestTradePips = bestTradePips,
                BestTradeUsd = bestTradeUsd,
                ExpectancyPips = expectancyPips,
                ExpectancyUsd = expectancyUsd,
                LongsCount = longsCount,
                LongsWon = longsWon,
                MaxDrawdown = maxDrawdown,
                MaxDrawdownDuration = maxDrawdownDuration,
                PerCrossStatistics = new JobGroupPerCrossStatisticsPartialViewModel() { PerCrossStatistics = perCrossStats },
                ProfitFactor = profitFactor,
                SharpeRatio = sharpeRatio,
                ShortsCount = shortsCount,
                ShortsWon = shortsWon,
                StandardDeviationPips = standardDeviationPips,
                StandardDeviationUsd = standardDeviationUsd,
                TotalFees = totalFees,
                TotalPips = totalPips,
                TotalVolume = totalVolume,
                WorstTradePips = worstTradePips,
                WorstTradeUsd = worstTradeUsd
            };
        }

        private List<JobGroupPerCrossStatisticsViewModel> ComputePerCrossStats(List<BacktestTradeModel> trades)
        {
            var groupings = trades.GroupBy(t => t.Cross);

            return groupings.Select(g => new JobGroupPerCrossStatisticsViewModel()
            {
                Pair = g.Key,
                TotalFeesUsd = g.Select(t => t.CommissionUsd ?? 0).Sum(),
                TotalGrossUsd = g.Select(t => t.RealizedPnlUsd ?? 0).Sum(),
                TotalPips = g.Select(t => t.RealizedPnlPips ?? 0).Sum(),
                TradesCount = g.Count(t => t.IsPositionClosing() || t.IsPositionContinuing()),
                Volume = g.Select(t => t.SizeUsd ?? 0).Sum() * 1000
            }).OrderBy(c => c.Pair).ToList();
        }

        internal string ExportListToExcel(List<BacktestJobGroupModel> jobGroups)
        {
            string fileName = $"BacktestJobs.{new Random().Next(10000)}.xlsx";
            string fullPath = Path.Combine(Path.GetTempPath(), fileName);

            byte[] bytes = CreateExcel(jobGroups);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                stream.WriteTo(file);
                file.Close();
            }

            return fileName;
        }

        private byte[] CreateExcel(List<BacktestJobGroupModel> jobGroups)
        {
            if (jobGroups.IsNullOrEmpty())
                return new byte[0];

            using (ExcelPackage excel = new ExcelPackage())
            {
                // Create the worksheet
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Jobs");

                var paramsDict = ComputeParamsDict(jobGroups, 6);

                // Header
                ws.Cells[1, 1].Value = "Net PnL (USD)";
                ws.Cells[1, 2].Value = "Start Date";
                ws.Cells[1, 3].Value = "End Date";
                ws.Cells[1, 4].Value = "Start Time";
                ws.Cells[1, 5].Value = "End Time";

                if (!paramsDict.IsNullOrEmpty())
                {
                    foreach (var kvp in paramsDict)
                        ws.Cells[1, kvp.Value].Value = kvp.Key;
                }

                // Jobs
                int rowCount = 2;
                foreach (var jobGroup in jobGroups)
                {
                    ws.Cells[rowCount, 1].Value = jobGroup.NetRealizedPnlUsd;
                    ws.Cells[rowCount, 2].Value = jobGroup.StartDate.ToLocalTime().ToString("dd/MM/yyyy");
                    ws.Cells[rowCount, 3].Value = jobGroup.EndDate.ToLocalTime().ToString("dd/MM/yyyy");
                    ws.Cells[rowCount, 4].Value = jobGroup.StartTime.ToLocalTime().ToString("HH:mm");
                    ws.Cells[rowCount, 5].Value = jobGroup.EndTime.ToLocalTime().ToString("HH:mm");

                    if (jobGroup.Strategy != null && !jobGroup.Strategy.Parameters.IsNullOrEmpty())
                    {
                        foreach (var parameter in jobGroup.Strategy.Parameters)
                        {
                            if (double.TryParse(parameter.Value, out double dblValue))
                                ws.Cells[rowCount, paramsDict[parameter.Name]].Value = dblValue;
                            else
                                ws.Cells[rowCount, paramsDict[parameter.Name]].Value = parameter.Value;
                        }
                    }

                    rowCount++;
                }

                var allRange = ws.Cells[1, 1, ws.Dimension.Rows, ws.Dimension.Columns];

                // Create Table
                var table = ws.Tables.Add(allRange, "Table");
                table.ShowTotal = false;
                table.StyleName = "White";
                table.TableStyle = TableStyles.Light1;

                // Formatting
                ws.Column(1).Style.Numberformat.Format = "#,##0.00";
                ws.Column(2).Style.Numberformat.Format = "dd/MM/yyyy";
                ws.Column(3).Style.Numberformat.Format = "dd/MM/yyyy";
                ws.Column(4).Style.Numberformat.Format = "HH:mm";
                ws.Column(5).Style.Numberformat.Format = "HH:mm";

                // Add colors
                var condFormattingRule = ws.ConditionalFormatting.AddThreeColorScale(ws.Cells[2, 1, ws.Dimension.Rows, 1]);
                condFormattingRule.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                condFormattingRule.MiddleValue.Value = 0;

                // Finally autofit all columns
                allRange.AutoFitColumns();

                return excel.GetAsByteArray();
            }
        }

        private Dictionary<string, int> ComputeParamsDict(List<BacktestJobGroupModel> jobGroups, int colOffset)
        {
            List<string> paramsList = new List<string>();

            foreach (var jobGroup in jobGroups)
            {
                if (jobGroup.Strategy != null && !jobGroup.Strategy.Parameters.IsNullOrEmpty())
                {
                    var parameters = jobGroup.Strategy.Parameters.Select(p => p.Name);

                    foreach (var parameter in parameters)
                    {
                        if (!paramsList.Contains(parameter))
                            paramsList.Add(parameter);
                    }
                }
            }

            paramsList.Sort();

            return paramsList.ToDictionary(p => p, p => paramsList.IndexOf(p) + colOffset);
        }

        internal FileResult DownloadExcelList(string fileName)
        {
            string fullPath = Path.Combine(Path.GetTempPath(), fileName);
            string contentType = "Application/msexcel";

            FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(fs, contentType)
            {
                FileDownloadName = "BacktestJobs.xlsx"
            };
        }

        private class BacktestJobGroupCreationDateComparer : IComparer<BacktestJobGroup>
        {
            private static BacktestJobGroupCreationDateComparer instance;

            public static BacktestJobGroupCreationDateComparer Instance
            {
                get
                {
                    if (instance == null)
                        instance = new BacktestJobGroupCreationDateComparer();

                    return instance;
                }
            }

            private BacktestJobGroupCreationDateComparer() { }

            public int Compare(BacktestJobGroup x, BacktestJobGroup y)
            {
                return x.CreateTime.CompareTo(y.CreateTime);
            }
        }
    }

    //public class MaxDrawDownCalculator
    //{
    //    private double maxDrawdown = 0;
    //    private int maxDrawdownIndex = 0;

    //    public (double, TimeSpan) ComputeMaxDrawDown(IEnumerable<BacktestTradeModel> trades)
    //    {
    //        var tradesList = trades.OrderBy(t => t.Timestamp).ToList();



    //        //List<double> pnlsSteps = new List<double>();

    //        //for (int i = 1; i < tradesList.Count; i++)
    //        //    pnlsSteps.Add(tradesList[i].RealizedPnlUsd.Value - tradesList[i - 1].RealizedPnlUsd.Value);

    //        //List<int> inversions = new List<int>();

    //        //int curSign = 0;

    //        //for (int i = 0; i < pnlsSteps.Count; i++)
    //        //{
    //        //    int sign = Math.Sign(pnlsSteps[0]);

    //        //    if (sign != 0 && sign != curSign)
    //        //    {
    //        //        inversions.Add(i);
    //        //        curSign = sign;
    //        //    }
    //        //}

    //        //for (int i = 1; i < inversions.Count; i++)
    //        //{
    //        //}
    //    }
    //}
}
