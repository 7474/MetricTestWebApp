using App.Metrics;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Json;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Formatters.Prometheus.Internal;
using App.Metrics.Formatters.Prometheus.Internal.Extensions;
using App.Metrics.Logging;
using AppMetricsMackerelReporter.Extensions;
using AppMetricsMackerelReporter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsMackerelReporter
{
    public class HostMetricsJsonOutputFormatter : IMetricsOutputFormatter
    {
        private static readonly ILog Logger = LogProvider.For<HostMetricsJsonOutputFormatter>();

        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _hostId;

        public MetricsMediaTypeValue MediaType => new MetricsMediaTypeValue("application", "vnd.appmetrics.metrics", "v1", "json");
        public MetricFields MetricFields { get; set; }

        public HostMetricsJsonOutputFormatter(string hostId)
        {
            _serializerSettings = DefaultJsonSerializerSettings.CreateSerializerSettings();
            _hostId = hostId;
        }

        public Task WriteAsync(
            Stream output,
            MetricsDataValueSource metricData,
            CancellationToken cancellationToken)
        {
            var baseDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var time = (metricData.Timestamp - baseDate).Ticks / 10000000;

            var metrics = metricData.GetMackerelMetricsSnapshot()
                .SelectMany(metricGroup =>
                {
                    return metricGroup.metric.SelectMany(metricValue =>
                    {
                        return ToMetricValues(_hostId, time, metricGroup, metricValue);
                    });
                });

            using (var streamWriter = new StreamWriter(output))
            {
                using (var textWriter = new JsonTextWriter(streamWriter))
                {
                    var serilizer = JsonSerializer.Create(_serializerSettings);
                    serilizer.Serialize(textWriter, metrics);
                }
            }
            Logger.Debug(string.Join(",", metrics.Select(x => x.name)));

            return Task.CompletedTask;
        }

        private static IEnumerable<HostMetricValue> ToMetricValues(string hostId, long time, MetricGroup metricGroup, MetricValue metricValue)
        {
            return metricGroup.metric.Select(x => ToHostMetricValue(hostId, time, metricGroup, x));
        }

        private static HostMetricValue ToHostMetricValue(string hostId, long time, MetricGroup metricGroup, MetricValue metricValue)
        {
            return new HostMetricValue()
            {
                hostId = hostId,
                time = time,
                name = metricGroup.name + (string.IsNullOrEmpty(metricValue.name) ? "" : "." + metricValue.name.NoamalizeName()),
                value = metricValue.value
            };
        }
    }
}
