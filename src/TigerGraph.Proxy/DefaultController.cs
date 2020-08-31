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
    public class DefaultController : ControllerBase
    {
        private HttpProxyOptions _httpOptions = HttpProxyOptionsBuilder.Instance
        .WithShouldAddForwardedHeaders(false)
        .WithHttpClientName("MyCustomClient")
        .WithIntercept(async context =>
        {
            if (context.Connection.RemotePort == 7777)
            {
                context.Response.StatusCode = 300;
                await context.Response.WriteAsync("I don't like this port, so I am not proxying this request!");
                return true;
            }

            return false;
        })
        .WithBeforeSend((c, hrm) =>
        {
            // Set something that is needed for the downstream endpoint.
            hrm.Headers.Authorization = new AuthenticationHeaderValue("Basic");

            return Task.CompletedTask;
        })
        .WithAfterReceive((c, hrm) =>
        {
            // Alter the content in  some way before sending back to client.
            var newContent = new StringContent("It's all greek...er, Latin...to me!");
            hrm.Content = newContent;

            return Task.CompletedTask;
        })
        .WithHandleFailure(async (c, e) =>
        {
            // Return a custom error response.
            c.Response.StatusCode = 403;
            await c.Response.WriteAsync("Things borked.");
        }).Build();

        [Route("p/{**rest}")]
        public Task ProxyCatchAll(string rest)
        {
            return this.HttpProxyAsync($"https://jsonplaceholder.typicode.com/{rest}");
        }
    }
}
