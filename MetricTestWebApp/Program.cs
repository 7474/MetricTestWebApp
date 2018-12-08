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
using AppMetricsMackerelReporter;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace MetricTestWebApp
{
    public class Program
    {
        public static IMetricsRoot Metrics { get; private set; }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .WriteTo.LiterateConsole()
                .CreateLogger();

            // ホストのビルドパイプライン中で設定を参照するためここで設定をビルドする。
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            string mackerelApiKey = config.GetValue<string>("Mackerel:ApiKey");
            string mackerelHostId = config.GetValue<string>("Mackerel:HostId");
            //builder.Report.ToConsole(TimeSpan.FromSeconds(30));
            //builder.Report.ToTextFile(@"C:\metrics.txt", TimeSpan.FromSeconds(20));

            var filter = new MetricsFilter(); //.WhereType(App.Metrics.MetricType.Timer);
            Metrics = AppMetrics.CreateDefaultBuilder()
                .Report.ToMackerel(
                    options =>
                    {
                        options.ApiKey = mackerelApiKey;
                        options.HostId = mackerelHostId;

                        options.HttpOptions.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                        options.HttpOptions.HttpPolicy.FailuresBeforeBackoff = 3;
                        options.HttpOptions.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                        options.HttpOptions.Filter = filter;
                        options.HttpOptions.FlushInterval = TimeSpan.FromSeconds(60);
                    })
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                //.UseMetrics()
                //.UseMetricsWebTracking()
                .UseStartup<Startup>();
        }
    }

}
