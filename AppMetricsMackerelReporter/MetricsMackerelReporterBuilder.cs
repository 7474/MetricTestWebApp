using App.Metrics;
using App.Metrics.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppMetricsMackerelReporter
{

    /// <summary>
    ///     Builder for configuring metrics Mackerel reporting using an
    ///     <see cref="IMetricsReportingBuilder" />.
    /// </summary>
    public static class MetricsMackerelReporterBuilder
    {
        public static IMetricsBuilder ToMackerel(
            this IMetricsReportingBuilder metricReporterProviderBuilder,
            Action<MetricsReportingMackerelOptions> setupAction)
        {
            if (metricReporterProviderBuilder == null)
            {
                throw new ArgumentNullException(nameof(metricReporterProviderBuilder));
            }

            var options = new MetricsReportingMackerelOptions();

            setupAction?.Invoke(options);

            var provider = new MackerelMetricsReporter(options);

            return metricReporterProviderBuilder.Using(provider);
        }
    }
}
