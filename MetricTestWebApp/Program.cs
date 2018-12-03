using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Filtering;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Json;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Formatters.Prometheus.Internal;
using App.Metrics.Formatters.Prometheus.Internal.Extensions;
using App.Metrics.Internal;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MetricTestWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            // XXX ええ感じの取りかた
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureMetricsWithDefaults(builder =>
                {
                    string mackerelApiKey = config.GetValue<string>("Meckerel:ApiKey");
                    string mackerelHostId = config.GetValue<string>("Mackerel:HostId");
                    //builder.Report.ToConsole(TimeSpan.FromSeconds(2));
                    //builder.Report.ToTextFile(@"C:\metrics.txt", TimeSpan.FromSeconds(20));
                    var filter = new MetricsFilter().WhereType(App.Metrics.MetricType.Timer);
                    builder.Report.OverHttp(
                        options =>
                        {
                            options.HttpSettings.RequestUri = new Uri("https://api.mackerelio.com/api/v0/tsdb");
                            options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                            options.HttpPolicy.FailuresBeforeBackoff = 3;
                            options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                            options.MetricsOutputFormatter = new HostMetricsJsonOutputFormatter(mackerelHostId);
                            options.InnerHttpMessageHandler = new MackerelApiMessageHandler(mackerelApiKey);
                            options.Filter = filter;
                            options.FlushInterval = TimeSpan.FromSeconds(60);
                        });
                })
                .UseMetrics()
                .UseMetricsWebTracking()
                .UseStartup<Startup>();
        }
    }

    // https://docs.microsoft.com/ja-jp/aspnet/web-api/overview/advanced/http-message-handlers
    public class MackerelApiMessageHandler : DelegatingHandler
    {
        private readonly string _apiKey;
        public MackerelApiMessageHandler(string apiKey)
        {
            InnerHandler = new HttpClientHandler();
            _apiKey = apiKey;
        }
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("X-Api-Key", _apiKey);
            Console.WriteLine("Send Host Metric. " + DateTimeOffset.UtcNow);
            return base.SendAsync(request, cancellationToken);
        }
    }
    public class HostMetricValue
    {
        public string hostId { get; set; }
        public string name { get; set; }
        public long time { get; set; }
        public decimal value { get; set; }
    }
    public class HostMetricsJsonOutputFormatter : IMetricsOutputFormatter
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _hostId;

        public HostMetricsJsonOutputFormatter(string hostId)
        {
            _serializerSettings = DefaultJsonSerializerSettings.CreateSerializerSettings();
            _hostId = hostId;
        }

        public MetricsMediaTypeValue MediaType => new MetricsMediaTypeValue("application", "vnd.appmetrics.metrics", "v1", "json");

        public Task WriteAsync(
            Stream output,
            MetricsDataValueSource metricData,
            CancellationToken cancellationToken)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            var baseDt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var time = (metricData.Timestamp - baseDt).Ticks / 10000000;
            var serilizer = JsonSerializer.Create(_serializerSettings);
            var metrics = metricData.GetPrometheusMetricsSnapshot(PrometheusFormatterConstants.MetricNameFormatter)
                .SelectMany(metricFamiry =>
                {
                    return metricFamiry.metric.SelectMany(metric =>
                    {
                        return ToMetricValues(metricFamiry, metric);
                    });
                }).Select(x =>
                {
                    x.hostId = _hostId;
                    x.time = time;
                    return x;
                });

            using (var streamWriter = new StreamWriter(output))
            {
                using (var textWriter = new JsonTextWriter(streamWriter))
                {
                    serilizer.Serialize(textWriter, metrics);
                }
            }
            Console.WriteLine(string.Join(", ", metrics.Select(x => x.name)));

#if NETSTANDARD1_6
            return Task.CompletedTask;
#else
            return AppMetricsTaskHelper.CompletedTask();
#endif
        }
        private static IEnumerable<HostMetricValue> ToMetricValues(MetricFamily family, Metric metric)
        {
            var s = new List<HostMetricValue>();
            var familyName = family.name;
            // Mackerelのホストメトリックではサーバに関するメタ情報を送出する必要はない
            var label = new LabelPair[] { };// metric.label;

            if (metric.gauge != null)
            {
                s.Add(SimpleValue(familyName, metric.gauge.value, label));
            }
            else if (metric.counter != null)
            {
                s.Add(SimpleValue(familyName, metric.counter.value, label));
            }
            else if (metric.summary != null)
            {
                s.Add(SimpleValue(familyName, metric.summary.sample_sum, label, ".sum"));
                s.Add(SimpleValue(familyName, metric.summary.sample_count, label, ".count"));

                foreach (var quantileValuePair in metric.summary.quantile)
                {
                    var quantile = double.IsPositiveInfinity(quantileValuePair.quantile)
                        ? "+Inf"
                        : quantileValuePair.quantile.ToString(CultureInfo.InvariantCulture);
                    s.Add(
                        SimpleValue(
                            familyName,
                            quantileValuePair.value,
                            label.Concat(new[] { new LabelPair { name = "quantile", value = quantile } })));
                }
            }
            else if (metric.histogram != null)
            {
                s.Add(SimpleValue(familyName, metric.histogram.sample_sum, label, ".sum"));
                s.Add(SimpleValue(familyName, metric.histogram.sample_count, label, ".count"));
                foreach (var bucket in metric.histogram.bucket)
                {
                    var value = double.IsPositiveInfinity(bucket.upper_bound) ? "+Inf" : bucket.upper_bound.ToString(CultureInfo.InvariantCulture);
                    s.Add(
                        SimpleValue(
                            familyName,
                            bucket.cumulative_count,
                            label.Concat(new[] { new LabelPair { name = "le", value = value } }),
                            ".bucket"));
                }
            }
            else
            {
                // not supported
            }

            return s;
        }
        private static string WithLabels(string familyName, IEnumerable<LabelPair> labels)
        {
            var labelPairs = labels as LabelPair[] ?? labels.ToArray();

            if (labelPairs.Length == 0)
            {
                return familyName;
            }

            return string.Format("{0}.{1}", familyName, string.Join(".", labelPairs.Select(l => string.Format("{0}{1}", l.name, l.value).Replace(".", ""))));
        }
        private static HostMetricValue SimpleValue(string family, double value, IEnumerable<LabelPair> labels, string namePostfix = null)
        {
            return new HostMetricValue()
            {
                name = WithLabels(family.Replace('_', '.') + (namePostfix ?? string.Empty), labels),
                value = new decimal(value)
            };
        }
    }
}
