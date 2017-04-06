using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.WebApi;
using Capital.GSG.FX.Trading.Strategy;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using DataTypes.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Backtester.Server.ControllerUtils
{
    public class CreateJobControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<CreateJobControllerUtils>();

        private const string AlgorithmSuffix = "Algorithm";
        private const string ParamKey = "Param";
        private const string StrategySuffix = "Strategy";
        private const string TooltipKey = "Tooltip";

        private readonly string stratFilesUploadDirectory;
        private readonly JobsControllerUtils jobsControllerUtils;
        private readonly JobGroupsControllerUtils jobGroupsControllerUtils;

        private ConcurrentDictionary<string, BacktestJobSettingsModel> jobs = new ConcurrentDictionary<string, BacktestJobSettingsModel>();

        private int newJobCounter = 1;
        private object newJobCounterLocker = new object();

        public CreateJobControllerUtils(string stratFilesUploadDirectory, JobsControllerUtils jobsControllerUtils, JobGroupsControllerUtils jobGroupsControllerUtils)
        {
            this.stratFilesUploadDirectory = !string.IsNullOrEmpty(stratFilesUploadDirectory) ? stratFilesUploadDirectory : Path.GetTempPath();
            this.jobsControllerUtils = jobsControllerUtils;
            this.jobGroupsControllerUtils = jobGroupsControllerUtils;
        }

        public string GetFilePath(string uploadedFileName)
        {
            string randomSuffix = $"{DateTimeOffset.Now.ToString("yyMMddHHmmssfff")}{(new Random()).Next(100000)}";

            string fileExt = uploadedFileName.Substring(uploadedFileName.Length - 4);

            return Path.Combine(stratFilesUploadDirectory, $"{uploadedFileName.Substring(0, uploadedFileName.Length - 4)}.{randomSuffix}{fileExt}");
        }

        public CreateJobStep1ViewModel ListStratProperties(string dllPath)
        {
            CreateJobStep1ViewModel result = new CreateJobStep1ViewModel();

            result.Settings.NewFileName = dllPath;

            try
            {
                logger.Debug($"Loading DLL from {dllPath}");

                Assembly assembly = Assembly.LoadFile(dllPath);

                // 1. Strategy
                Type strategyType = assembly.GetTypes()?.Where(t => t.Name.EndsWith(StrategySuffix)).FirstOrDefault();

                IStrategy strategyInstance = null;

                if (strategyType != null)
                {
                    result.Settings.StrategyClass = strategyType.FullName;

                    strategyInstance = Activator.CreateInstance(strategyType) as IStrategy;

                    if (strategyInstance != null)
                    {
                        result.Settings.Crosses = strategyInstance.Crosses.ToList();
                        result.Settings.StrategyName = strategyInstance.Name;
                        result.Settings.StrategyVersion = strategyInstance.Version;
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
                    result.Settings.AlgorithmClass = algorithmType.FullName;

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

                    List<BacktestJobStrategyParameterModel> properties = (from p in algorithmType.GetProperties()
                                                                          where p.Name.StartsWith(ParamKey)
                                                                          select new BacktestJobStrategyParameterModel()
                                                                          {
                                                                              Name = p.Name.Remove(0, ParamKey.Length),
                                                                              Value = algoInstance != null ? p.GetValue(algoInstance).ToString() : null
                                                                          })?.OrderBy(p => p.Name).ToList();

                    if (!properties.IsNullOrEmpty())
                    {
                        IEnumerable<Tuple<string, string>> tooltips = (from p in algorithmType.GetProperties()
                                                                       where p.Name.StartsWith(TooltipKey)
                                                                       select new Tuple<string, string>(
                                                                           p.Name.Remove(0, TooltipKey.Length),
                                                                           algoInstance != null ? p.GetValue(algoInstance) as string : null))
                                                                           .Where(t => !string.IsNullOrEmpty(t.Item2));

                        if (!tooltips.IsNullOrEmpty())
                        {
                            foreach (Tuple<string, string> tooltip in tooltips)
                            {
                                var param = properties.FirstOrDefault(p => p.Name == tooltip.Item1);

                                if (param != null)
                                    param.Tooltip = tooltip.Item2;
                            }
                        }

                        result.Settings.Parameters = properties;
                        result.Success = true;

                        do
                        {
                            result.Settings.JobName = GetNextJobName(result.Settings.StrategyName, result.Settings.StrategyVersion);
                        } while (!jobs.TryAdd(result.Settings.JobName, result.Settings));

                        return result;
                    }
                    else
                    {
                        string err = $"No strategy parameter found. Is this intentional? Param names must start with prefix '{ParamKey}' to be listed here";
                        logger.Error(err);

                        result.Success = false;
                        result.Message = err;

                        return result;
                    }
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
            catch (Exception ex)
            {
                logger.Error($"Failed to load properties for type {AlgorithmSuffix} from DLL {dllPath}", ex);

                string err = $"Failed to retrieve list of strat params: {ex.Message}";

                result.Success = false;
                result.Message = err;

                return result;
            }
        }

        private string GetNextJobName(string stratName, string stratVersion)
        {
            lock (newJobCounterLocker)
            {
                newJobCounter++;
            };

            return $"job_{newJobCounter}_{stratName}_{stratVersion}_{DateTimeOffset.Now:yyyyMMddHHmmss}".Replace(" ", "").Replace(".", "");
        }

        internal async Task<CreateJobStep1ViewModel> DuplicateJob(string jobNameToDuplicate)
        {
            logger.Info($"Will attempt to duplicate job {jobNameToDuplicate}");

            var jobGroup = await jobGroupsControllerUtils.Get(jobNameToDuplicate);

            if (jobGroup != null && jobGroup.Strategy != null)
            {
                string dllPath = jobGroup.Strategy.StrategyDllPath;

                if (File.Exists(dllPath))
                {
                    var result = new CreateJobStep1ViewModel()
                    {
                        Message = $"Duplicate of {jobNameToDuplicate}",
                        Settings = new BacktestJobSettingsModel()
                        {
                            AlgorithmClass = jobGroup.Strategy?.AlgoTypeName,
                            Crosses = jobGroup.Strategy.Crosses,
                            EndDate = jobGroup.EndDate.LocalDateTime,
                            EndTime = jobGroup.EndTime.LocalDateTime,
                            NewFileName = dllPath,
                            OriginalFileName = jobGroup.Strategy.StrategyDllPath,
                            Parameters = jobGroup.Strategy.Parameters.ToBacktestJobStrategyParameterModels("Param"),
                            StartDate = jobGroup.StartDate.LocalDateTime,
                            StartTime = jobGroup.StartTime.LocalDateTime,
                            StrategyClass = jobGroup.Strategy.StrategyTypeName,
                            StrategyName = jobGroup.Strategy.Name,
                            StrategyVersion = jobGroup.Strategy.Version,
                            UseHistoDatabase = jobGroup.UseHistoDatabase
                        },
                        Success = true
                    };

                    do
                    {
                        result.Settings.JobName = GetNextJobName(result.Settings.StrategyName, result.Settings.StrategyVersion);
                    } while (!jobs.TryAdd(result.Settings.JobName, result.Settings));

                    return result;
                }
                else
                {
                    logger.Error($"Unable to the DLL file used for previous job {jobNameToDuplicate} ({dllPath}). Will create a new job instead");
                    return new CreateJobStep1ViewModel();
                }
            }
            else
            {
                logger.Error($"Unable to retrieve details of job {jobNameToDuplicate}. Will create a new one instead");
                return new CreateJobStep1ViewModel();
            }
        }

        public BacktestJobSettingsModel GetJobSettings(string jobName)
        {
            BacktestJobSettingsModel settings;

            if (!jobs.TryGetValue(jobName, out settings))
            {
                logger.Error($"Failed to retrieve settings for job {jobName}");
                settings = new BacktestJobSettingsModel()
                {
                    JobName = jobName,
                    Parameters = new List<BacktestJobStrategyParameterModel>()
                };
            }

            return settings;
        }

        public BacktestJobSettingsModel SetParameters(string jobName, List<BacktestJobStrategyParameterModel> parameters)
        {
            return jobs.AddOrUpdate(jobName, (key) => null, (key, oldValue) =>
            {
                oldValue.Parameters = parameters;

                return oldValue;
            });
        }

        public BacktestJobSettingsModel SetTimeRange(string jobName, DateTime startDate, DateTime endDate, DateTime startTime, DateTime endTime, bool useHistoDatabase)
        {
            return jobs.AddOrUpdate(jobName, (key) => null, (key, oldValue) =>
            {
                oldValue.StartDate = startDate;
                oldValue.EndDate = endDate;
                oldValue.StartTime = startTime;
                oldValue.EndTime = endTime;
                oldValue.UseHistoDatabase = useHistoDatabase;

                return oldValue;
            });
        }

        internal async Task<CreateJobSubmitViewModel> CreateJob(string jobName)
        {
            try
            {
                var jobSettings = GetJobSettings(jobName);

                if (jobSettings == null)
                    return new CreateJobSubmitViewModel(jobName, false, $"Failed to get settings for job {jobName}");

                if (string.IsNullOrEmpty(jobSettings?.StrategyName))
                    throw new ArgumentNullException("StrategyName");

                if (string.IsNullOrEmpty(jobSettings?.StrategyVersion))
                    throw new ArgumentNullException("StrategyVersion");

                if (string.IsNullOrEmpty(jobSettings?.StrategyClass))
                    throw new ArgumentNullException("StrategyClass");

                if (jobSettings.Crosses.IsNullOrEmpty())
                    throw new ArgumentNullException("Crosses");

                if (jobSettings.Parameters.IsNullOrEmpty())
                    throw new ArgumentNullException("Parameters");

                var days = DateTimeUtils.EachBusinessDay(jobSettings.StartDate, jobSettings.EndDate);

                if (days.IsNullOrEmpty())
                    throw new ArgumentOutOfRangeException(nameof(days), $"Invalid days interval: start {jobSettings.StartDate}, end: {jobSettings.EndDate}");

                var jobGroup = new BacktestJobGroup(jobName)
                {
                    EndDate = jobSettings.EndDate,
                    EndTime = jobSettings.EndTime,
                    StartDate = jobSettings.StartDate,
                    StartTime = jobSettings.StartTime,
                    Strategy = new Strategy()
                    {
                        AlgoTypeName = jobSettings.AlgorithmClass,
                        Crosses = jobSettings.Crosses,
                        Name = jobSettings.StrategyName,
                        Parameters = jobSettings.Parameters.ToStrategyParameters("Param"),
                        StrategyDllPath = jobSettings.NewFileName,
                        StrategyTypeName = jobSettings.StrategyClass,
                        Version = jobSettings.StrategyVersion
                    },
                    UseHistoDatabase = jobSettings.UseHistoDatabase
                };

                List<GenericActionResult> failed = new List<GenericActionResult>();

                int dayCounter = 1;
                foreach (var day in days)
                {
                    BacktestJob job = new BacktestJob(jobName, $"{jobName}_{dayCounter++}")
                    {
                        Day = day,
                        EndTime = jobSettings.EndTime,
                        StartTime = jobSettings.StartTime
                    };

                    logger.Info($"Inserting new backtest job {job.Name} in dictionary database");

                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(5));

                    var addResult = await jobsControllerUtils.AddJob(job);

                    if (addResult.Success)
                        jobGroup.Jobs.Add(job.Name, new BacktestJobLight() { DayStr = day.ToString("yyyy-MM-dd") });
                    else
                        failed.Add(addResult);
                }

                if (!jobGroup.Jobs.IsNullOrEmpty() && failed.IsNullOrEmpty())
                {
                    var result = await jobGroupsControllerUtils.AddJobGroup(jobGroup);

                    if (result.Success)
                        return new CreateJobSubmitViewModel(jobName, true, $"Successfully split backtest job {jobName} into {jobGroup.Jobs.Count} subjobs ({string.Join(", ", jobGroup.Jobs.Keys)}) and submitted.");
                    else
                        return new CreateJobSubmitViewModel(jobName, false, $"Failed to submit group job {jobName}: {result.Message}");
                }
                else
                    return new CreateJobSubmitViewModel(jobName, false, $"Failed to submit one or more subjobs: {string.Join(", ", failed.Select(f => f.Message))}");
            }
            catch (ArgumentNullException ex)
            {
                string err = $"Failed to insert backtest job: missing or invalid parameter {ex.ParamName}";
                logger.Error(err, ex);

                return new CreateJobSubmitViewModel(jobName, false, err);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to insert backtest job", ex);

                return new CreateJobSubmitViewModel(jobName, false, $"Failed to insert backtest job: {ex.Message}");
            }
        }
    }
}
