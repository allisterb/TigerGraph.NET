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
        #region Constructors
        public ProxyController(ILogger<ProxyController> logger)
        {
            log = logger;
        }

        #endregion

        #region Actions
        [Route("p/{**rest}")]
        public Task RestServerProxy(string rest)
        {
            log.LogInformation("Proxying request {0} to {1}...", rest, $"{Program.TG_REST_SERVER_URL}{rest}");
            return this.HttpProxyAsync($"{Program.TG_REST_SERVER_URL}/{rest}", _restServerHttpOptions);
        }
        #endregion

        #region Fields
        private static ILogger<ProxyController> log;

        private HttpProxyOptions _restServerHttpOptions = HttpProxyOptionsBuilder.Instance
        .WithShouldAddForwardedHeaders(false)
        .WithHttpClientName("TigerGraphClient")
        .WithIntercept(context =>
        {
            return new ValueTask<bool>(false);
        })
        .WithBeforeSend((c, hrm) =>
        {
            hrm.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Program.TG_TOKEN);
            return Task.CompletedTask;
        })
        .WithAfterReceive((c, hrm) =>
        {
            log.LogInformation("Proxy to {0} succeeded.", c.Request.Query);
            return Task.CompletedTask;
        })
        .WithHandleFailure((c, e) =>
        {
            log.LogError(e, "Proxy to {0} error.", c.Request.Query);
            return Task.CompletedTask;
        }).Build();
        #endregion
    }

}
