using Backtester.Server.Models;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class TradesControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<TradesControllerUtils>();

        private readonly JobsControllerUtils jobsControllerUtils;
        private readonly JobGroupsControllerUtils jobGroupsControllerUtils;

        public TradesControllerUtils(JobsControllerUtils jobsControllerUtils, JobGroupsControllerUtils jobGroupsControllerUtils)
        {
            this.jobsControllerUtils = jobsControllerUtils;
            this.jobGroupsControllerUtils = jobGroupsControllerUtils;
        }

        internal GenericActionResult HandleNewTrade(string backtestJobName, BacktestTrade trade)
        {
            if (trade == null)
                return new GenericActionResult(false, "Invalid trade object: null");

            logger.Debug($"Processing new trade {trade.TradeId}");

            return jobsControllerUtils.AddTrade(backtestJobName, trade);
        }

        internal async Task<FileResult> ExportExcel(string jobGroupId)
        {
            string fileName = "Trades.xlsx";
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
            var trades = await jobGroupsControllerUtils.GetTrades(jobGroupId);

            if (trades.IsNullOrEmpty())
                return new byte[0];

            string[] headers = new string[13]
            {
                "Execution Time (HKT)",
                "Origin",
                "Side",
                "Quantity",
                "Pair",
                "Rate",
                "PnL (USD)",
                "PnL per Hour (USD)",
                "PnL (pips)",
                "PnL per Hour (pips)",
                "Net Cumul PnL (USD)",
                "Duration",
                "Fees (USD)"
            };

            using (ExcelPackage excel = new ExcelPackage())
            {
                List<object[]> dataset = new List<object[]>();

                foreach (var trade in trades)
                {
                    try
                    {
                        object[] row = new object[13];

                        row[0] = trade.Timestamp.ToLocalTime();
                        row[1] = trade.OrderOrigin.ToString();
                        row[2] = trade.Side.ToString();
                        row[3] = trade.Size * 1000;
                        row[4] = trade.Cross.ToString();
                        row[5] = trade.Price;
                        row[6] = trade.RealizedPnlUsd;
                        row[7] = trade.RealizedPnlUsdPerHour;
                        row[8] = trade.RealizedPnlPips;
                        row[9] = trade.RealizedPnlPipsPerHour;
                        row[10] = trade.NetCumulPnlUsd;
                        row[11] = trade.Duration.HasValue ? trade.Duration.Value.ToString(@"hh\:mm\:ss") : null;
                        row[12] = trade.CommissionUsd;

                        dataset.Add(row);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to add trade to the dataset", ex);
                    }
                }

                // Create the worksheet
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Trades");

                ws.Cells["A1"].LoadFromArrays(new string[1][] { headers });
                ws.Cells["A2"].LoadFromArrays(dataset);

                // Format numbers
                #region Timestamp
                using (ExcelRange col = ws.Cells[2, 1, 2 + dataset.Count(), 1])
                {
                    col.Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                }
                #endregion

                #region Quantity
                using (ExcelRange col = ws.Cells[2, 4, 2 + dataset.Count(), 4])
                {
                    col.Style.Numberformat.Format = "#,##0";
                }
                #endregion

                #region Rate
                using (ExcelRange col = ws.Cells[2, 6, 2 + dataset.Count(), 6])
                {
                    col.Style.Numberformat.Format = (trades.First().Cross == Cross.USDJPY) ? "0.00" : "0.00000";
                }
                #endregion

                #region PnL (USD)
                using (ExcelRange col = ws.Cells[2, 7, 2 + dataset.Count(), 7])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }

                using (ExcelRange col = ws.Cells[2, 8, 2 + dataset.Count(), 8])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }

                using (ExcelRange col = ws.Cells[2, 11, 2 + dataset.Count(), 11])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }
                #endregion

                #region PnL (pips)
                using (ExcelRange col = ws.Cells[2, 9, 2 + dataset.Count(), 9])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }

                using (ExcelRange col = ws.Cells[2, 10, 2 + dataset.Count(), 10])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }
                #endregion

                #region Commission
                using (ExcelRange col = ws.Cells[2, 13, 2 + dataset.Count(), 13])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                }
                #endregion

                // Format as table
                //var tableRng = ws.Cells[1, 1, dataset.Count() + 1, headers.Length];
                //var table = ws.Tables.Add(tableRng, $"Trades-{cross}");
                //table.TableStyle = TableStyles.Light9;
                //table.ShowHeader = true;
                //table.ShowTotal = true;
                //table.Columns["Position"].TotalsRowLabel = cumulativePosition.ToString();
                //table.Columns["PnL"].TotalsRowFormula = "SUBTOTAL(109,[PnL])";
                //table.Columns["PnL Ccy"].TotalsRowLabel = pnlCcy;
                //table.Columns["PnL USD"].TotalsRowFormula = "SUBTOTAL(109,[PnL USD])";
                //table.Columns["Commission"].TotalsRowFormula = "SUBTOTAL(109,[Commission])";
                //table.Columns["Commission Ccy"].TotalsRowLabel = commissionCcy;
                //table.Columns["Commission USD"].TotalsRowFormula = "SUBTOTAL(109,[Commission USD])";

                // Finally autofit all columns
                ws.Cells[1, 1, dataset.Count() + 1, headers.Length].AutoFitColumns();

                return excel.GetAsByteArray();
            }
        }
    }
}
