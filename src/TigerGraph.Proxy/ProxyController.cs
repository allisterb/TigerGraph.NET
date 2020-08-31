using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Serilog;
using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;

namespace TigerGraph.Proxy
{
    public class ProxyController : ControllerBase
    {
        private static ILogger<ProxyController> log;
        public ProxyController(ILogger<ProxyController> logger)
        {
            log = logger;
        }

        private HttpProxyOptions _httpOptions = HttpProxyOptionsBuilder.Instance
        .WithShouldAddForwardedHeaders(false)
        .WithHttpClientName("TigerGraphClient")
        .WithIntercept(context =>
        {
            return new ValueTask<bool>(false);
        })
        .WithBeforeSend((c, hrm) =>
        {
            hrm.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("TG_TOKEN"));

            return Task.CompletedTask;
        })
        .WithAfterReceive((c, hrm) =>
        {
            hrm.Headers.Add("Access-Control-Allow-Origin", "*");
            hrm.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            hrm.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            log.LogInformation("Proxy succeeded and Access-Control-* headers added to response.");
            return Task.CompletedTask;
        })
        .WithHandleFailure((c, e) =>
        {
            log.LogError(e, "Proxy to {0} error.", c.Request.Query);
            return Task.CompletedTask;
        }).Build();

        [Route("p/{**rest}")]
        public Task Proxy(string rest)
        {
            log.LogInformation("Proxying request {0} to {1}...", rest, $"{Environment.GetEnvironmentVariable("TG_SERVER_URL")}/{rest}");
            return this.HttpProxyAsync($"{Environment.GetEnvironmentVariable("TG_SERVER_URL")}/{rest}", _httpOptions);
        }
    }
}
