using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public class Startup
    {
        readonly string AllowAnyPolicy = "AllowAnyPolicy";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowAnyPolicy,
                                  builder =>
                                  {
                                      builder
                                        .AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader();
                                  });
            });
            services.AddRouting();
            services.AddProxies();
            services.AddControllers();
            services.AddHttpClient("TigerGraphClient", c => c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TG_SERVER_URL")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseCors(AllowAnyPolicy);
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

    }
}
