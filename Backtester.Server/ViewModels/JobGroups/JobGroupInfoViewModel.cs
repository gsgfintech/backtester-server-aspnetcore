using Backtester.Server.Models;
using DataTypes.Core;
using System.Text;

namespace Backtester.Server.ViewModels.JobGroups
{
    public class JobGroupInfoViewModel
    {
        public BacktestJobGroupModel JobGroup { get; private set; }

        public string ClipboardContent { get; private set; }

        public JobGroupInfoViewModel(BacktestJobGroup jobGroup)
        {
            JobGroup = jobGroup.ToBacktestJobGroupModel();

            BuildClipboardContent();
        }

        private void BuildClipboardContent()
        {
            if (JobGroup != null)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Info");
                sb.AppendLine($"ID,{JobGroup.GroupId}");
                sb.AppendLine($"Status,{JobGroup.Status}");
                sb.AppendLine($"Time Created (HKT),{JobGroup.CreateTime}");
                sb.AppendLine($"Actual Start Time (HKT),{JobGroup.ActualStartTime}");
                sb.AppendLine($"Time Completed (HKT),{JobGroup.CompletionTime}");
                sb.AppendLine($"Duration,{JobGroup.Duration}");
                sb.AppendLine("Timeframe");
                sb.AppendLine($"Test Start Date,{JobGroup.StartDate}");
                sb.AppendLine($"Test End Date,{JobGroup.EndDate}");
                sb.AppendLine($"Test Start Time,{JobGroup.StartTime}");
                sb.AppendLine($"Test End Time,{JobGroup.EndTime}");
                sb.AppendLine("Strategy");
                sb.AppendLine($"Name,{JobGroup.Strategy.Name}");
                sb.AppendLine($"Version,{JobGroup.Strategy.Version}");
                sb.AppendLine($"DLL Path,{JobGroup.Strategy.DllPath}");
                sb.AppendLine($"Strategy Type Name,{JobGroup.Strategy.StrategyTypeName}");
                sb.AppendLine($"Algo Type Name,{JobGroup.Strategy.AlgoTypeName}");
                sb.AppendLine($"Crosses,{string.Join(", ", JobGroup.Strategy.Crosses)}");
                sb.AppendLine("Parameters");

                foreach (var parameter in JobGroup.Strategy.Parameters)
                    sb.AppendLine($"{parameter.Name},{parameter.Value}");

                ClipboardContent = sb.ToString();
            }
        }
    }
}
