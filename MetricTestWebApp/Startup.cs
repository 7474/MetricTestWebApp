using App.Metrics;
using App.Metrics.Filtering;
using AppMetricsMackerelReporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportingSandbox.JustForTesting;
using System;

namespace MetricTestWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            string mackerelApiKey = Configuration.GetValue<string>("Mackerel:ApiKey");
            string mackerelHostId = Configuration.GetValue<string>("Mackerel:HostId");
            var filter = new MetricsFilter();
            var metrics = AppMetrics.CreateDefaultBuilder()
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

            SampleMetricsRunner.ScheduleSomeSampleMetrics(metrics);
            services.AddMetrics(metrics);
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();
            services.AddMetricsEndpoints();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddMetrics();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetricsAllMiddleware();
            app.UseMetricsAllEndpoints();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
