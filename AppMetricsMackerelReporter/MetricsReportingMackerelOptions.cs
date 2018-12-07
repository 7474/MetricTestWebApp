using App.Metrics.Filtering;
using App.Metrics.Filters;
using App.Metrics.Reporting.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppMetricsMackerelReporter
{
    public class MetricsReportingMackerelOptions
    {
        public const string API_BASE_DEFAULT = "https://api.mackerelio.com/api/v0";

        /// <summary>
        /// The default value is <see cref="MetricsReportingMackerelOptions.API_BASE_DEFAULT"/>.
        /// </summary>
        public string ApiBase { get; set; }

        public string ApiKey { get; set; }

        public string HostId { get; set; }

        /// <summary>
        /// Mackerel specific settings will be overwritten.
        /// </summary>
        public MetricsReportingHttpOptions HttpOptions { get; set; }

        public MetricsReportingMackerelOptions()
        {
            ApiBase = API_BASE_DEFAULT;
            HttpOptions = new MetricsReportingHttpOptions()
            {
                // メトリックは1分毎に送出する
                FlushInterval = TimeSpan.FromSeconds(60)
            };
        }
    }
}
