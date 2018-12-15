using App.Metrics.Counter;
using App.Metrics.Meter;
using AppMetricsMackerelReporter.Models;

namespace AppMetricsMackerelReporter.Extensions
{
    public static class MetricSetItemExtensions
    {
        public static MetricValue ToMackerelMetric(this CounterValue.SetItem item)
        {
            var result = new MetricValue
            {
                name = item.Item,
                value = item.Count
            };

            return result;
        }

        public static MetricValue ToMackerelMetric(this MeterValue.SetItem item)
        {
            var result = new MetricValue
            {
                name = item.Item,
                value = item.Value.Count
            };

            return result;
        }
    }
}