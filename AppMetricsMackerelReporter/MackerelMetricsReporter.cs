using App.Metrics;
using App.Metrics.Filters;
using App.Metrics.Formatters;
using App.Metrics.Logging;
using App.Metrics.Reporting;
using App.Metrics.Reporting.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsMackerelReporter
{
    public class MackerelMetricsReporter : IReportMetrics
    {
        private static readonly ILog Logger = LogProvider.For<MackerelMetricsReporter>();

        public MetricsReportingMackerelOptions Options { get; private set; }
        private IReportMetrics _httpReporter;

        public MackerelMetricsReporter(MetricsReportingMackerelOptions options)
        {
            this.Options = options;

            var httpOptions = Options.HttpOptions;
            // https://mackerel.io/ja/api-docs/entry/host-metrics#post
            httpOptions.HttpSettings.RequestUri = new Uri(Options.ApiBase + "/tsdb");
            httpOptions.MetricsOutputFormatter = new HostMetricsJsonOutputFormatter(Options.HostId);
            httpOptions.InnerHttpMessageHandler = new MackerelApiMessageHandler(Options.ApiKey);

            _httpReporter = new HttpMetricsReporter(httpOptions);
        }

        // 処理は HttpMetricsReporter に委譲する。
        public IFilterMetrics Filter { get { return _httpReporter.Filter; } set { _httpReporter.Filter = value; } }
        public TimeSpan FlushInterval { get { return _httpReporter.FlushInterval; } set { _httpReporter.FlushInterval = value; } }
        public IMetricsOutputFormatter Formatter { get { return _httpReporter.Formatter; } set { _httpReporter.Formatter = value; } }
        public async Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await _httpReporter.FlushAsync(metricsData, cancellationToken);
            return result;
        }
    }
}
