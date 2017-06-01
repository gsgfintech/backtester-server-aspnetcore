using Backtester.Server.ControllerUtils;
using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJobExcel;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Trading.Strategy;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Backtester.Server.Controllers.CreateJobExcel
{
    public class CreateJobExcelControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<CreateJobExcelControllerUtils>();

        private readonly CreateJobControllerUtils createJobControllerUtils;
        private readonly string excelJobFilesUploadDirectory;
        private readonly string stratFilesUploadDirectory;

        private const string AlgorithmSuffix = "Algorithm";
        private const string StrategySuffix = "Strategy";

        public CreateJobExcelControllerUtils(CreateJobControllerUtils createJobControllerUtils, string excelJobFilesUploadDirectory, string stratFilesUploadDirectory)
        {
            this.createJobControllerUtils = createJobControllerUtils;
            this.excelJobFilesUploadDirectory = excelJobFilesUploadDirectory;
            this.stratFilesUploadDirectory = stratFilesUploadDirectory;
        }

        public string GetFilePath(string uploadedFileName)
        {
            string randomSuffix = $"{DateTimeOffset.Now.ToString("yyMMddHHmmssfff")}{(new Random()).Next(100000)}";

            string fileExt = uploadedFileName.Substring(uploadedFileName.Length - 4);

            return Path.Combine(excelJobFilesUploadDirectory, $"{uploadedFileName.Substring(0, uploadedFileName.Length - 4)}.{randomSuffix}.{fileExt}");
        }

        public (bool Success, string Message, List<BacktestJobSettingsModel> JobsSettings) ReadFile(string filePath)
        {
            (bool Success, string Message, List<BacktestJobSettingsModel> JobsSettings) result = (false, null, null);

            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));

                if (!File.Exists(filePath))
                {
                    result.Message = $"File {filePath} does not exist";
                    return result;
                }

                using (ExcelPackage excel = new ExcelPackage(new FileInfo(filePath)))
                {
                    var ws = excel.Workbook.Worksheets.FirstOrDefault();

                    if (ws == null)
                    {
                        result.Message = "Excel file is empty";
                        return result;
                    }

                    // Validate header
                    if (ws.Dimension.End.Row < 2)
                    {
                        result.Message = $"Incorrect length of the Excel file: expected at least 1 header row and 1 job row";
                        return result;
                    }

                    if (ws.Dimension.End.Column < 6)
                    {
                        result.Message = $"Incorrect length of the Excel file: expected at least 5 columns (not counting strat parameters)";
                        return result;
                    }

                    if (ws.Cells[1, 1].Value.ToString() != "DLL")
                    {
                        result.Message = $"Incorrect value for column 1: expected 'DLL'";
                        return result;
                    }

                    if (ws.Cells[1, 2].Value.ToString() != "StartDate")
                    {
                        result.Message = $"Incorrect value for column 2: expected 'StartDate'";
                        return result;
                    }

                    if (ws.Cells[1, 3].Value.ToString() != "EndDate")
                    {
                        result.Message = $"Incorrect value for column 3: expected 'EndDate'";
                        return result;
                    }

                    if (ws.Cells[1, 4].Value.ToString() != "StartTime")
                    {
                        result.Message = $"Incorrect value for column 4: expected 'StartTime'";
                        return result;
                    }

                    if (ws.Cells[1, 5].Value.ToString() != "EndTime")
                    {
                        result.Message = $"Incorrect value for column 5: expected 'EndTime'";
                        return result;
                    }

                    if (ws.Cells[1, 6].Value.ToString() != "UseHistoDatabase")
                    {
                        result.Message = $"Incorrect value for column 6: expected 'UseHistoDatabase'";
                        return result;
                    }

                    List<BacktestJobSettingsModel> jobsSettings = new List<BacktestJobSettingsModel>();

                    int rowIndex = 2;
                    while (ws.Cells[rowIndex, 1].Value != null)
                    {
                        #region DLL
                        string dll = ws.Cells[rowIndex, 1].Value?.ToString();
                        if (string.IsNullOrEmpty(dll))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing value for 'DLL'", null);

                        var readDllResult = ReadDll(dll);

                        if (!readDllResult.Success)
                        {
                            result.Message = $"Failed to read DLL: {readDllResult.Message}";
                            logger.Error(result.Message);
                            return result;
                        }
                        #endregion

                        #region StartDate
                        if (string.IsNullOrEmpty(ws.Cells[rowIndex, 2].Value.ToString()) || !DateTime.TryParse(ws.Cells[rowIndex, 2].Value.ToString(), out DateTime startDate))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing or invalid value for 'StartDate'", null);

                        #endregion

                        #region EndDate
                        if (string.IsNullOrEmpty(ws.Cells[rowIndex, 3].Value.ToString()) || !DateTime.TryParse(ws.Cells[rowIndex, 3].Value.ToString(), out DateTime endDate))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing or invalid value for 'EndDate'", null);

                        #endregion

                        #region StartTime
                        if (string.IsNullOrEmpty(ws.Cells[rowIndex, 4].Value.ToString()) || !DateTime.TryParse(ws.Cells[rowIndex, 4].Value.ToString(), out DateTime startTime))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing or invalid value for 'StartTime'", null);

                        #endregion

                        #region EndTime
                        if (string.IsNullOrEmpty(ws.Cells[rowIndex, 5].Value.ToString()) || !DateTime.TryParse(ws.Cells[rowIndex, 5].Value.ToString(), out DateTime endTime))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing or invalid value for 'EndTime'", null);
                        #endregion

                        #region UseHistoDatabase
                        if (string.IsNullOrEmpty(ws.Cells[rowIndex, 6].Value.ToString()) || !bool.TryParse(ws.Cells[rowIndex, 6].Value.ToString(), out bool useHistoDatabase))
                            return (false, $"Failed to parse job definition on row {rowIndex}: missing or invalid value for 'UseHistoDatabase'", null);
                        #endregion

                        #region Parameters
                        Dictionary<string, BacktestJobStrategyParameterModel> parameters = new Dictionary<string, BacktestJobStrategyParameterModel>();

                        for (int colIndex = 7; colIndex <= ws.Dimension.End.Column; colIndex++)
                        {
                            string key = ws.Cells[1, colIndex].Value.ToString();
                            string value = ws.Cells[rowIndex, colIndex].Value.ToString();

                            if (!string.IsNullOrEmpty(key) && !parameters.ContainsKey(key))
                                parameters.Add(key, new BacktestJobStrategyParameterModel()
                                {
                                    Name = key,
                                    Tooltip = null,
                                    Value = value
                                });
                        }
                        #endregion

                        var jobSettings = new BacktestJobSettingsModel()
                        {
                            AlgorithmClass = readDllResult.AlgorithmClass,
                            Crosses = readDllResult.Crosses,
                            EndDate = endDate,
                            EndTime = endTime,
                            NewFileName = readDllResult.DllFileName,
                            OriginalFileName = readDllResult.DllFileName,
                            Parameters = parameters.Values.ToList(),
                            StartDate = startDate,
                            StartTime = startTime,
                            StrategyClass = readDllResult.StrategyClass,
                            StrategyName = readDllResult.StrategyName,
                            StrategyVersion = readDllResult.StrategyVersion,
                            UseHistoDatabase = useHistoDatabase
                        };

                        jobSettings.JobName = createJobControllerUtils.AssignJobNameAndAddToDictionary(jobSettings);

                        jobsSettings.Add(jobSettings);

                        rowIndex++;
                    }

                    if (jobsSettings.Count > 0)
                    {
                        result.Success = true;
                        result.Message = $"Parsed {jobsSettings.Count} jobs";
                        result.JobsSettings = jobsSettings;
                    }
                    else
                        result.Message = "Found no job in the file";

                    return result;
                }
            }
            catch (ArgumentNullException ex)
            {
                result.Message = $"Not reading Excel file: missing or invalid parameter {ex.ParamName}";
                logger.Error(result.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Message = "Failed to read Excel file";
                logger.Error(result.Message, ex);
                return result;
            }
        }

        internal async Task<(bool Success, string Message, List<string> JobNames)> CreateJobs(string[] jobNames)
        {
            return await createJobControllerUtils.CreateJobs(jobNames);
        }

        private (bool Success, string Message, string StrategyClass, string AlgorithmClass, List<Cross> Crosses, string StrategyName, string StrategyVersion, string DllFileName) ReadDll(string filePath)
        {
            (bool Success, string Message, string StrategyClass, string AlgorithmClass, List<Cross> Crosses, string StrategyName, string StrategyVersion, string DllFileName) result = (false, null, null, null, null, null, null, null);

            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));

                filePath = filePath.Split('\\').Last();

                filePath = Path.Combine(stratFilesUploadDirectory, filePath);

                if (!File.Exists(filePath))
                {
                    result.Message = $"File {filePath} does not exist";
                    return result;
                }

                result.DllFileName = filePath;

                logger.Debug($"Loading DLL from {filePath}");

                Assembly assembly = Assembly.LoadFile(filePath);

                // 1. Strategy
                Type strategyType = assembly.GetTypes()?.Where(t => t.Name.EndsWith(StrategySuffix)).FirstOrDefault();

                IStrategy strategyInstance = null;

                if (strategyType != null)
                {
                    result.StrategyClass = strategyType.FullName;

                    strategyInstance = Activator.CreateInstance(strategyType) as IStrategy;

                    if (strategyInstance != null)
                    {
                        result.Crosses = strategyInstance.Crosses.ToList();
                        result.StrategyName = strategyInstance.Name;
                        result.StrategyVersion = strategyInstance.Version;
                    }
                    else
                    {
                        string err = "Failed to instanciate strategy from DLL";
                        logger.Error(err);

                        result.Success = false;
                        result.Message = err;

                        return result;
                    }
                }
                else
                {
                    string err = $"Failed to load strategy type from DLL. The name of the strategy class must end with suffix '{StrategySuffix}' to be taken into account";
                    logger.Error(err);

                    result.Success = false;
                    result.Message = err;

                    return result;
                }

                // 2. Algorithm
                Type algorithmType = assembly.GetTypes()?.Where(t => t.Name.EndsWith(AlgorithmSuffix)).FirstOrDefault();

                if (algorithmType != null)
                {
                    result.AlgorithmClass = algorithmType.FullName;

                    // Try to instanciate the algorithm to get the default value of its properties
                    object algoInstance = null;

                    try
                    {
                        algoInstance = Activator.CreateInstance(algorithmType);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to instanciate algorithm of type {algorithmType.FullName}. Won't be able to determine the default value of its properties", ex);
                    }

                    result.Success = true;

                    return result;
                }
                else
                {
                    string err = $"Failed to load algorithm type from DLL. The name of the algorithm class must end with suffix '{AlgorithmSuffix}' to be taken into account";
                    logger.Error(err);

                    result.Success = false;
                    result.Message = err;

                    return result;
                }
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Not reading DLL file: missing or invalid parameter {ex.ParamName}";
                logger.Error(err);
                return result;
            }
            catch (Exception ex)
            {
                string err = "Failed to read DLL file";
                logger.Error(err, ex);
                return result;
            }
        }
    }
}
