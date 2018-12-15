using App.Metrics;
using AppMetricsMackerelReporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetricsMackerelReporter.Extensions
{
    public static class MetricDataValueSourceExtensions
    {
        public static IEnumerable<MetricGroup> GetMackerelMetricsSnapshot(
            this MetricsDataValueSource snapshot)
        {
            var result = new List<MetricGroup>();
            foreach (var group in snapshot.Contexts)
            {
                foreach (var metricGroup in group.ApdexScores.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, metricGroup.Key),
                        type = MetricType.Gauge
                    };
                    foreach (var metric in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(metric.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }

                foreach (var metricGroup in group.Gauges.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, metricGroup.Key),
                        type = MetricType.Gauge
                    };
                    foreach (var metric in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(metric.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }

                foreach (var metricGroup in group.Counters.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, metricGroup.Key),
                        type = MetricType.Gauge
                    };

                    foreach (var metric in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(metric.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }

                foreach (var metricGroup in group.Meters.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, $"{metricGroup.Key}_total"),
                        type = MetricType.Counter
                    };

                    foreach (var metric in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(metric.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }

                foreach (var metricGroup in group.Histograms.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, metricGroup.Key),
                        type = MetricType.Histogram
                    };

                    foreach (var timer in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(timer.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }

                foreach (var metricGroup in group.Timers.GroupBy(
                    source => source.IsMultidimensional ? source.MultidimensionalName : source.Name))
                {
                    var mackerelMetricGroup = new MetricGroup
                    {
                        name = ToMetricName(group.Context, metricGroup.Key),
                        type = MetricType.Timer
                    };

                    foreach (var timer in metricGroup)
                    {
                        mackerelMetricGroup.metric.AddRange(timer.ToMackerelMetrics());
                    }

                    result.Add(mackerelMetricGroup);
                }
            }

            return result;
        }

        private static string ToMetricName(string context, string key)
        {
            return string.IsNullOrEmpty(context)
                ? key.NoamalizeName()
                : context.NoamalizeName() + "." + key.NoamalizeName();
        }
    }
}