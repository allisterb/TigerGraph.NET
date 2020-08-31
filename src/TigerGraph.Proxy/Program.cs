using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
namespace TigerGraph.Proxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new LoggerConfiguration();
            Log.Logger = config.MinimumLevel.Debug().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TG_TOKEN")))
            {
                Log.Error("The environment variable {0} could not be read.", "TG_TOKEN");
                return;
            }
            else if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TG_SERVER_URL")))
            {
                Log.Error("The environment variable {0} could not be read.", "TG_SERVER_URL");
            }
            else
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
