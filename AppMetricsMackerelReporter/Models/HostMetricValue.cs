using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable IDE1006

namespace AppMetricsMackerelReporter.Models
{
    /// <summary>
    /// <seealso cref="https://mackerel.io/ja/api-docs/entry/host-metrics#post"/>
    /// </summary>
    public class HostMetricValue
    {
        public string hostId { get; set; }
        public string name { get; set; }
        public long time { get; set; }
        public decimal value { get; set; }
    }
}
