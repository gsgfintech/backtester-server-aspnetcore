using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.SystemData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Backtester.Server.Models
{
    public class BacktesterWorkerModel
    {
        [Display(Name= "Current Job")]
        public string CurrentJob { get; set; }

        [Display(Name= "Accepting Jobs")]
        public bool IsAcceptingJobs { get; set; }

        [Display(Name = "Running")]
        public bool IsRunning { get; set; }

        public string Name { get; set; }

        public Datacenter Datacenter { get; set; }

        [Display(Name = "Start Time (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset StartTime { get; set; }

        [Display(Name = "Last Update (HKT)")]
        [DisplayFormat(DataFormatString = "{0:dd/MM HH:mm:ss}")]
        public DateTimeOffset LastHeardFrom { get; set; }

        [Display(Name = "Status")]
        public SystemStatusLevel? OverallStatus { get; set; }

        public List<SystemStatusAttributeModel> Attributes { get; set; }
    }

    public class SystemStatusAttributeModel
    {
        public SystemStatusLevel Level { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }

    public static class BacktesterWorkerModelExtensions
    {
        public static BacktesterWorkerModel ToBacktesterWorkerModel(this BacktesterWorker worker)
        {
            if (worker == null)
                return null;

            return new BacktesterWorkerModel()
            {
                Name = worker.Name,
                CurrentJob = worker.CurrentJob,
                IsAcceptingJobs = worker.IsAcceptingJobs,
                IsRunning = worker.IsRunning
            };
        }

        public static List<BacktesterWorkerModel> ToBacktesterWorkerModels(this IEnumerable<BacktesterWorker> workers)
        {
            return workers?.Select(worker => worker.ToBacktesterWorkerModel()).ToList();
        }

        private static SystemStatusAttributeModel ToSystemStatusAttributeModel(this SystemStatusAttribute attribute)
        {
            if (attribute == null)
                return null;

            return new SystemStatusAttributeModel()
            {
                Level = attribute.Level,
                Name = attribute.Name,
                Value = attribute.Value
            };
        }

        public static List<SystemStatusAttributeModel> ToSystemStatusAttributeModels(this IEnumerable<SystemStatusAttribute> attributes)
        {
            return attributes?.Select(a => a.ToSystemStatusAttributeModel()).ToList();
        }
    }
}
