using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
namespace TigerGraph.Proxy
{
    public class Program
    {
        #region Entry-point
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
                return;
            }
            else if (!Uri.TryCreate(Environment.GetEnvironmentVariable("TG_SERVER_URL"), UriKind.Absolute, out Uri u))
            {
                Log.Error("The environment variable {0}:{1} is not a valid URI.", "TG_SERVER_URL", Environment.GetEnvironmentVariable("TG_SERVER_URL"));
                return;
            }
            else
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("TG_TOKEN"));
                var pingTimer = new System.Timers.Timer(1000 * 60 * 15);
                pingTimer.Elapsed += (sender, e) => {
                    var now = DateTime.Now;
                    try
                    {
                        wc.DownloadString(u.ToString() + "echo");
                        Log.Information("Pinged {0} at {1}.", u, now);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error attempting to ping {0} at {1}.", u, now);
                    }
                };
                pingTimer.Enabled = true;
                CreateHostBuilder(args).Build().Run();
                wc.Dispose();
                pingTimer.Dispose();
            }
        }
        #endregion

        #region Properties
        public static string TG_USER = Environment.GetEnvironmentVariable("TG_USER");

        public static string TG_PASS = Environment.GetEnvironmentVariable("TG_PASS");

        public static string TG_TOKEN = Environment.GetEnvironmentVariable("TG_TOKEN");

        public static string TG_REST_SERVER_URL = Environment.GetEnvironmentVariable("TG_REST_SERVER_URL");

        public static string TG_GSQL_SERVER_URL = Environment.GetEnvironmentVariable("TG_GSQL_SERVER_URL");
        #endregion
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
