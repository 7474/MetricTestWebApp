using App.Metrics;
using System.Linq;

namespace AppMetricsMackerelReporter.Extensions
{
    public static class MetricTagExtensions
    {
        public static string ToName(this MetricTags tags)
        {
            return string.Join(".", tags.Keys.Select(x => x.NoamalizeName()));
        }
    }
}
