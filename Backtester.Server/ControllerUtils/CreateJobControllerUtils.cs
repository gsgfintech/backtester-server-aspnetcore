﻿using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Trading.Strategy;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

        private ConcurrentDictionary<string, BacktestJobSettingsModel> jobs = new ConcurrentDictionary<string, BacktestJobSettingsModel>();

        private int newJobCounter = 1;
        private object newJobCounterLocker = new object();

        public CreateJobControllerUtils(string stratFilesUploadDirectory)
        {
            this.stratFilesUploadDirectory = !string.IsNullOrEmpty(stratFilesUploadDirectory) ? stratFilesUploadDirectory : Path.GetTempPath();
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
                            result.Settings.JobName = $"job_{newJobCounter}_{result.Settings.StrategyName}_{result.Settings.StrategyVersion}_{DateTimeOffset.Now:yyyyMMddHHmmss}".Replace(" ", "").Replace(".", "");

                            lock (newJobCounterLocker)
                            {
                                newJobCounter++;
                            }
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

        public BacktestJobSettingsModel GetJobSettings(string jobName)
        {
            BacktestJobSettingsModel settings;

            if (!jobs.TryGetValue(jobName, out settings))
            {
                logger.Error($"Failed to retrieve settings for job {jobName}");
                settings = new BacktestJobSettingsModel()
                {
                    JobName = jobName
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
    }
}
