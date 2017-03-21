using Capital.GSG.FX.Data.Core.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backtester.Server.Models
{
    public class AlertModel
    {
        public string AlertId { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? ClosedTimestamp { get; set; }
        public AlertLevel Level { get; set; }
        public string Source { get; set; }
        public AlertStatus Status { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public static class AlertModelExtensions
    {
        public static AlertModel ToAlertModel(this Alert alert)
        {
            if (alert == null)
                return null;

            return new AlertModel()
            {
                AlertId = alert.AlertId,
                Body = alert.Body,
                ClosedTimestamp = alert.ClosedTimestamp,
                Level = alert.Level,
                Source = alert.Source,
                Status = alert.Status,
                Subject = alert.Subject,
                Timestamp = alert.Timestamp
            };
        }

        public static List<AlertModel> ToAlertModels(this IEnumerable<Alert> alerts)
        {
            return alerts?.Select(a => a.ToAlertModel()).ToList();
        }
    }
}
