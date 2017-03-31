using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Capital.GSG.FX.Backtest.DataTypes;
using Microsoft.Extensions.Logging;
using Capital.GSG.FX.Utils.Core.Logging;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using System.Collections.Concurrent;
using Capital.GSG.FX.Utils.Core;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;

namespace Backtester.Server.ControllerUtils
{
    public class TradeGenericMetric2SeriesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradeGenericMetric2SeriesControllerUtils>();

        private readonly TradeGenericMetric2SerieActioner actioner;

        private readonly ConcurrentDictionary<string, List<BacktestTradeGenericMetric2Serie>> seriesByJobGroupDict = new ConcurrentDictionary<string, List<BacktestTradeGenericMetric2Serie>>();

        public TradeGenericMetric2SeriesControllerUtils(TradeGenericMetric2SerieActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<(bool, string)> HandleNewTradeGenericMetric2Serie(BacktestTradeGenericMetric2Serie tradeGenericMetric2Serie)
        {
            try
            {
                if (tradeGenericMetric2Serie == null)
                    throw new ArgumentNullException(nameof(tradeGenericMetric2Serie));

                if (tradeGenericMetric2Serie.Points.IsNullOrEmpty())
                    return (false, "Unrealized PnL serie is empty");

                logger.Info($"Received new unrealized PnL serie of {tradeGenericMetric2Serie.Points.Count} points for job group {tradeGenericMetric2Serie.JobGroupId} (trade {tradeGenericMetric2Serie.TradeDescription})");

                // 1. Save in database
                var result = await actioner.Insert(tradeGenericMetric2Serie);

                if (!result.Item1)
                    return result;

                // 2. Add to in-memory dictionary
                seriesByJobGroupDict.AddOrUpdate(tradeGenericMetric2Serie.JobGroupId, (key) =>
                {
                    return new List<BacktestTradeGenericMetric2Serie>() { tradeGenericMetric2Serie };
                }, (key, oldValue) =>
                {
                    oldValue.Add(tradeGenericMetric2Serie);
                    return oldValue;
                });

                return (true, $"Successfully added new unrealized PnL serie for job group {tradeGenericMetric2Serie.JobGroupId}");
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not processing unrealized PnL serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return (false, $"{err}: {ex.Message}");
            }
            catch (Exception ex)
            {
                string err = "Failed to process unrealized PnL serie";
                logger.Error(err);
                return (false, $"{err}: {ex.Message}");
            }
        }

        internal async Task<List<BacktestTradeGenericMetric2Serie>> GetForJobGroup(string jobGroupId)
        {
            List<BacktestTradeGenericMetric2Serie> series;

            if (seriesByJobGroupDict.TryGetValue(jobGroupId, out series))
                return series;
            else
            {
                logger.Info($"Querying series for job group {jobGroupId} from database as they are not in the dictionary");

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var result = await actioner.GetAllForJobGroup(jobGroupId, cts.Token);

                if (!result.IsNullOrEmpty())
                    seriesByJobGroupDict.TryAdd(jobGroupId, result);

                return result;
            }
        }

        internal async Task<FileResult> ExportExcel(string jobGroupId)
        {
            string fileName = "Metric2s.xlsx";
            string contentType = "Application/msexcel";

            byte[] bytes = await CreateExcel(jobGroupId);

            MemoryStream stream = new MemoryStream(bytes);

            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private async Task<byte[]> CreateExcel(string jobGroupId)
        {
            var series = await GetForJobGroup(jobGroupId);

            if (series.IsNullOrEmpty())
                return new byte[0];

            Dictionary<int, Dictionary<int, double>> organizedSeries = OrganizeSeries(series);

            int maxTimeValue = organizedSeries.Keys.Max();

            using (ExcelPackage excel = new ExcelPackage())
            {
                // Create the worksheet
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Metric2s");

                int rowsCount = organizedSeries.Count;

                // 1. First column : time (in minutes)
                // 2. Second column : time (in seconds)
                ws.Cells[3, 1].Value = "Time (minutes)";
                ws.Cells[3, 2].Value = "Time (seconds)";

                // 3. Headers
                int curColumn = 3;
                foreach (var subKvp in organizedSeries[0])
                {
                    ws.Cells[1, curColumn].Value = series[subKvp.Key].TradeDescription;
                    ws.Cells[2, curColumn].Value = series[subKvp.Key].TradeCloseOrigin.ToString();
                    ws.Cells[3, curColumn++].Value = "Metric2";
                }

                // 4. Metric2 values
                int curRow = 4;
                var kvps = organizedSeries.OrderBy(kvp => kvp.Key);
                foreach (var kvp in kvps)
                {
                    ws.Cells[curRow, 1].Value = kvp.Key % 60 == 0 ? kvp.Key / 60 : kvp.Key / 60 + 1;
                    ws.Cells[curRow, 2].Value = kvp.Key;

                    curColumn = 3;
                    for (int i = 0; i < series.Count; i++)
                    {
                        if (kvp.Value.ContainsKey(i))
                            ws.Cells[curRow, curColumn].Value = kvp.Value[i];

                        curColumn++;
                    }

                    curRow++;
                }

                // Add colors
                for (int col = 3; col < series.Count + 3; col++)
                {
                    var condFormattingRule = ws.ConditionalFormatting.AddThreeColorScale(ws.Cells[4, col, rowsCount + 3, col]);
                    condFormattingRule.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                    condFormattingRule.MiddleValue.Value = 50;
                }

                // Finally autofit all columns
                ws.Cells[1, 1, rowsCount + 3, series.Count + 2].AutoFitColumns();

                // Group rows
                for (int i = 5; i < rowsCount + 5; i += 12)
                {
                    for (int j = i; j < i + 11; j++)
                    {
                        ws.Row(j).OutlineLevel = 1;
                        ws.Row(j).Collapsed = true;
                    }
                }

                return excel.GetAsByteArray();
            }
        }

        private Dictionary<int, Dictionary<int, double>> OrganizeSeries(List<BacktestTradeGenericMetric2Serie> series)
        {
            Dictionary<int, Dictionary<int, double>> organizedSeries = new Dictionary<int, Dictionary<int, double>>();

            // Put series in dictionary
            for (int i = 0; i < series.Count; i++)
            {
                for (int j = 0; j < series[i].Points.Count; j++)
                {
                    int key = series[i].Points[j].TimeInSeconds;

                    if (!organizedSeries.ContainsKey(key))
                        organizedSeries.Add(key, new Dictionary<int, double>() { { i, series[i].Points[j].GenericMetric2Value } });
                    else
                        organizedSeries[key][i] = series[i].Points[j].GenericMetric2Value;
                }
            }

            // Add missing keys
            int maxTimeValue = organizedSeries.Keys.Max();

            for (int i = 0; i < maxTimeValue; i += 5)
            {
                if (!organizedSeries.ContainsKey(i))
                    organizedSeries.Add(i, new Dictionary<int, double>());
            }

            return organizedSeries;
        }
    }
}
