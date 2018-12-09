using App.Metrics.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetricsMackerelReporter
{
    // https://docs.microsoft.com/ja-jp/aspnet/web-api/overview/advanced/http-message-handlers
    class MackerelApiMessageHandler : DelegatingHandler
    {
        private static readonly ILog Logger = LogProvider.For<MackerelApiMessageHandler>();

        private readonly string _apiKey;

        public MackerelApiMessageHandler(string apiKey)
        {
            InnerHandler = new HttpClientHandler();
            _apiKey = apiKey;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.Debug("Send Host Metric. " + DateTimeOffset.UtcNow);

            request.Headers.Add("X-Api-Key", _apiKey);
            var response = await base.SendAsync(request, cancellationToken);

            // コンテンツを呼び出し元で使う雰囲気はないのでここでデバッグ用に読んでしまう
            Logger.Debug("Send Host Metric..." + response.StatusCode + " " + await response.Content.ReadAsStringAsync());
            return response;
        }
    }
}
