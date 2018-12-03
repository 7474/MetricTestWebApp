using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureMetricsWithDefaults(builder =>
                    {
                        builder.Report.ToConsole(TimeSpan.FromSeconds(2));
                        //builder.Report.ToTextFile(@"C:\metrics.txt", TimeSpan.FromSeconds(20));
                    })  
                .UseMetrics()
                .UseMetricsWebTracking()
                .UseStartup<Startup>();
        }
    }
}
