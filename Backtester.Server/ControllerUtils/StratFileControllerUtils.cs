using Backtester.Server.Models;
using Backtester.Server.ViewModels.CreateJob;
using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Trading.Strategy;
using Capital.GSG.FX.Utils.Core;
using Capital.GSG.FX.Utils.Core.Logging;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Backtester.Server.ControllerUtils
{
    public class StratFileControllerUtils
    {
        private readonly ILogger logger = GSGLoggerFactory.Instance.CreateLogger<StratFileControllerUtils>();

        private const string AlgorithmSuffix = "Algorithm";
        private const string ParamKey = "Param";
        private const string StrategySuffix = "Strategy";
        private const string TooltipKey = "Tooltip";

        private readonly string stratFilesUploadDirectory;

        public StratFileControllerUtils(string stratFilesUploadDirectory)
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

                AssemblyLoader assemblyLoader = new AssemblyLoader();
                Assembly assembly = assemblyLoader.LoadFromAssemblyPath(dllPath);

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
                                                                              Type = p.PropertyType.Name,
                                                                              DefaultValue = algoInstance != null ? p.GetValue(algoInstance).ToString() : null
                                                                          })?.ToList();

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
    }

    public class AssemblyLoader : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var deps = DependencyContext.Default;
            var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
            var assembly = Assembly.Load(new AssemblyName(res.First().Name));
            return assembly;
        }
    }
}
