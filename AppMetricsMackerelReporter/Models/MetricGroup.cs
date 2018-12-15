using App.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable IDE1006

namespace AppMetricsMackerelReporter.Models
{
    public class MetricGroup
    {
        public MetricType type { get; set; }
        public string name { get; set; }
        public List<MetricValue> metric { get; set; }

        public MetricGroup()
        {
            metric = new List<MetricValue>();
        }
    }
}
