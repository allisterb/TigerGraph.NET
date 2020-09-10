using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;

namespace TigerGraph.Proxy
{
    public class ProxyController : ControllerBase
    {
        #region Constructors
        public ProxyController(ILogger<ProxyController> logger, TigerGraphCache c)
        {
            log = logger;
            cache = c;
        }
        #endregion

        #region Actions
        [Route("p/{**rest}")]
        public Task RestServerProxy(string rest)
        {
            log.LogInformation("Proxying request {0} to {1}...", rest, $"{Program.TG_REST_SERVER_URL}/{rest}");
            return this.HttpProxyAsync($"{Program.TG_REST_SERVER_URL}/{rest}",
                HttpProxyOptionsBuilder.Instance
                .WithShouldAddForwardedHeaders(false)
                .WithHttpClientName("TigerGraphClient")
                .WithIntercept(async context =>
                {
                    if (cache.Cache.TryGetValue(rest, out string json))
                    {
                        await context.Content(json, "application/json");
                        log.LogInformation("Cache hit for path {0}.", rest);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                })
                .WithBeforeSend((c, hrm) =>
                {
                    hrm.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Program.TG_TOKEN);
                    return Task.CompletedTask;
                })
                .WithAfterReceive(async (context, hrm) =>
                {
                    var options = new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
                    cache.Cache.Set(rest, await hrm.Content.ReadAsStringAsync(), options);
                    log.LogInformation("Proxy to {0} succeeded. Added path {1} to cache.", context.Request.Query, rest);
                })
                .WithHandleFailure((c, e) =>
                {
                    log.LogError(e, "Proxy to {0} error.", c.Request.Query);
                    return Task.CompletedTask;
                })
                .Build()
            );
        }
        #endregion

        #region Fields
        private static ILogger<ProxyController> log;
        private TigerGraphCache cache;
        #endregion
    }

}
