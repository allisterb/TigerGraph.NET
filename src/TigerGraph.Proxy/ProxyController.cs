using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;


using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;

namespace TigerGraph.Proxy
{
    public class ProxyController : ControllerBase
    {
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
            return Task.CompletedTask;
        })
        .WithHandleFailure((c, e) =>
        {
            return Task.CompletedTask;
        }).Build();

        [Route("p/{**rest}")]
        public Task Proxy(string rest)
        {
            return this.HttpProxyAsync($"{Environment.GetEnvironmentVariable("TG_SERVER_URL")}/{rest}", _httpOptions);
        }
    }
}
