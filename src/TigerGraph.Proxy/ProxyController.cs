using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            hrm.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "qsaa6s3i8dhususu952jncku8jtim9b3");

            return Task.CompletedTask;
        })
        .WithAfterReceive((c, hrm) =>
        {
            // Alter the content in  some way before sending back to client.
            var newContent = new StringContent("It's all greek...er, Latin...to me!");
            hrm.Content = newContent;

            return Task.CompletedTask;
        })
        .WithHandleFailure((c, e) =>
        {
            return Task.CompletedTask;
            // Return a custom error response.
            //c.Response.StatusCode = 403;
            //await c.Response.WriteAsync("Things borked.");
        }).Build();

        [Route("p/{**rest}")]
        public Task Proxy(string rest)
        {
            return this.HttpProxyAsync($"https://fss.i.tgcloud.io:9000/{rest}");
        }
    }
}
