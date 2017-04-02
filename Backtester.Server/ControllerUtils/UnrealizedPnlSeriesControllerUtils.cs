using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Backtest.MongoConnector.Actioner;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Capital.GSG.FX.Data.Core.WebApi;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using OfficeOpenXml.ConditionalFormatting;

namespace Backtester.Server.ControllerUtils
{
    public class UnrealizedPnlSeriesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<UnrealizedPnlSeriesControllerUtils>();

        private readonly UnrealizedPnlSerieActioner actioner;

        public UnrealizedPnlSeriesControllerUtils(UnrealizedPnlSerieActioner actioner)
        {
            this.actioner = actioner;
        }

        internal async Task<GenericActionResult> HandleNewUnrealizedPnlSerie(BacktestUnrealizedPnlSerie pnlSerie)
        {
            try
            {
                if (pnlSerie == null)
                    throw new ArgumentNullException(nameof(pnlSerie));

                if (pnlSerie.Points.IsNullOrEmpty())
                    return new GenericActionResult(false, "Unrealized PnL serie is empty");

                logger.Info($"Received new unrealized PnL serie of {pnlSerie.Points.Count} points for job group {pnlSerie.JobGroupId} (trade {pnlSerie.TradeDescription})");

                var result = await actioner.Insert(pnlSerie);

                if (!result.Success)
                    return result;
                else
                    return new GenericActionResult(true, $"Successfully added new unrealized PnL serie for job group {pnlSerie.JobGroupId}");
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not processing unrealized PnL serie: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
            catch (Exception ex)
            {
                string err = "Failed to process unrealized PnL serie";
                logger.Error(err);
                return new GenericActionResult(false, $"{err}: {ex.Message}");
            }
        }

        internal async Task<List<BacktestUnrealizedPnlSerie>> GetForJobGroup(string jobGroupId)
        {
            logger.Info($"Querying series for job group {jobGroupId} from database");

            var series = await actioner.GetAllForJobGroup(jobGroupId);

            if (!series.IsNullOrEmpty())
                series.Sort(new Comparison<BacktestUnrealizedPnlSerie>((x, y) => x.TradeOpenTimestamp.CompareTo(y.TradeOpenTimestamp)));

            return series;
        }

        internal async Task<FileResult> ExportUnrPnLExcel(string jobGroupId)
        {
            string fileName = "UnrPnLs.xlsx";
            string contentType = "Application/msexcel";

            byte[] bytes = await CreateExcel(jobGroupId, false);

            MemoryStream stream = new MemoryStream(bytes);

            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        internal async Task<FileResult> ExportUnrPnLPerHourExcel(string jobGroupId)
        {
            string fileName = "UnrPnLsPerHour.xlsx";
            string contentType = "Application/msexcel";

            byte[] bytes = await CreateExcel(jobGroupId, true);

            MemoryStream stream = new MemoryStream(bytes);

            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private async Task<byte[]> CreateExcel(string jobGroupId, bool pnlPerHour)
        {
            var series = await GetForJobGroup(jobGroupId);

            if (series.IsNullOrEmpty())
                return new byte[0];

            Dictionary<int, Dictionary<int, double>> organizedSeries = OrganizeSeries(series);

            int maxTimeValue = organizedSeries.Keys.Max();

            using (ExcelPackage excel = new ExcelPackage())
            {
                // Create the worksheet
                string wsName = pnlPerHour ? "Unr PnLs/h" : "Unr PnLs";
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add(wsName);

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
                    ws.Cells[3, curColumn++].Value = "Unr PnL (pips)";
                }

                // 4. Unr PnLs values
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
                            ws.Cells[curRow, curColumn].Value = pnlPerHour ? ((double?)kvp.Key > 0 ? Math.Round(kvp.Value[i] / kvp.Key * 12 * 60, 1) : (double?)null) : kvp.Value[i];

                        curColumn++;
                    }

                    curRow++;
                }

                // Add colors
                for (int col = 3; col < series.Count + 3; col++)
                {
                    var condFormattingRule = ws.ConditionalFormatting.AddThreeColorScale(ws.Cells[4, col, rowsCount + 3, col]);
                    condFormattingRule.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                    condFormattingRule.MiddleValue.Value = 0;
                }

                // Autofit all columns
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

        private Dictionary<int, Dictionary<int, double>> OrganizeSeries(List<BacktestUnrealizedPnlSerie> series)
        {
            Dictionary<int, Dictionary<int, double>> organizedSeries = new Dictionary<int, Dictionary<int, double>>();

            // Put series in dictionary
            for (int i = 0; i < series.Count; i++)
            {
                for (int j = 0; j < series[i].Points.Count; j++)
                {
                    int key = series[i].Points[j].TimeInSeconds;

                    if (!organizedSeries.ContainsKey(key))
                        organizedSeries.Add(key, new Dictionary<int, double>() { { i, series[i].Points[j].UnrealizedPnlInPips } });
                    else
                        organizedSeries[key][i] = series[i].Points[j].UnrealizedPnlInPips;
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
