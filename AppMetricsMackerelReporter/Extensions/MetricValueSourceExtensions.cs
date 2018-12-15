using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Timer;
using AppMetricsMackerelReporter.Models;

namespace AppMetricsMackerelReporter.Extensions
{
    public static class MetricValueSourceExtensions
    {
        public static decimal ToDecimal(this double value)
        {
            try
            {
                return new decimal(value);
            }
            catch
            {
                return 0;
            }
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this ApdexValueSource metric)
        {
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "",
                    value = metric.Value.Score.ToDecimal()
                }
            };

            return result;
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this GaugeValueSource metric)
        {
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "",
                    value = metric.Value.ToDecimal()
                }
            };

            return result;
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this CounterValueSource metric)
        {
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "Meta.Count",
                    value = metric.Value.Count
                }
            };

            if (metric.Value.Items?.Length > 0)
            {
                result.AddRange(metric.Value.Items.Select(i => i.ToMackerelMetric()));
            }

            return result;
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this MeterValueSource metric)
        {
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "Meta.Count",
                    value = metric.Value.Count
                }
            };

            if (metric.Value.Items?.Length > 0)
            {
                result.AddRange(metric.Value.Items.Select(x => x.ToMackerelMetric()));
            }

            return result;
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this HistogramValueSource metric)
        {
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "Meta.Count",
                    value = metric.Value.Count
                },
                new MetricValue
                {
                    name = "Meta.Sum",
                    value = metric.Value.Sum.ToDecimal()
                },
                new MetricValue
                {
                    name = "Median",
                    value = metric.Value.Median.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile75",
                    value = metric.Value.Percentile75.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile95",
                    value = metric.Value.Percentile95.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile99",
                    value = metric.Value.Percentile99.ToDecimal()
                },
            };
            // new Quantile(){quantile = 0.98, value = metric.Value.Percentile98},
            // new Quantile(){quantile = 0.999, value = metric.Value.Percentile999}
            return result;
        }

        public static IEnumerable<MetricValue> ToMackerelMetrics(this TimerValueSource metric)
        {
            var rescaledVal = metric.Value.Scale(TimeUnit.Milliseconds, TimeUnit.Milliseconds);
            var result = new List<MetricValue>
            {
                new MetricValue
                {
                    name = "Meta.Count",
                    value = rescaledVal.Rate.Count
                },
                new MetricValue
                {
                    name = "Meta.Sum",
                    value = rescaledVal.Histogram.Sum.ToDecimal()
                },
                new MetricValue
                {
                    name = "Median",
                    value = rescaledVal.Histogram.Median.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile75",
                    value = rescaledVal.Histogram.Percentile75.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile95",
                    value = rescaledVal.Histogram.Percentile95.ToDecimal()
                },
                new MetricValue
                {
                    name = "Percentile99",
                    value = rescaledVal.Histogram.Percentile99.ToDecimal()
                },
            };
            // new Quantile(){quantile = 0.98, value = metric.Value.Histogram.Percentile98},
            // new Quantile(){quantile = 0.999, value = metric.Value.Histogram.Percentile999}
            return result;
        }
    }
}