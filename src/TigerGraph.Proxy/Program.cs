using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
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
            /* Configure the Serilog Logger */
            var config = new LoggerConfiguration();
            Log.Logger = config.MinimumLevel.Debug().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
           
            /* Check for required environment vars */
            if (string.IsNullOrEmpty(TG_TOKEN))
            {
                Log.Error("The environment variable {0} could not be read.", "TG_TOKEN");
                return;
            }
            else if (string.IsNullOrEmpty(TG_REST_SERVER_URL))
            {
                Log.Error("The environment variable {0} could not be read.", "TG_REST_SERVER_URL");
                return;
            }
            else if (string.IsNullOrEmpty(TG_GSQL_SERVER_URL))
            {
                Log.Error("The environment variable {0} could not be read.", "TG_GSQL_SERVER_URL");
                return;
            }
            else if (!Uri.TryCreate(TG_REST_SERVER_URL, UriKind.Absolute, out Uri rsu))
            {
                Log.Error("The environment variable {0}:{1} is not a valid URI.", "TG_REST_SERVER_URL", TG_REST_SERVER_URL);
                return;
            }
            else if (!Uri.TryCreate(TG_GSQL_SERVER_URL, UriKind.Absolute, out Uri gu))
            {
                Log.Error("The environment variable {0}:{1} is not a valid URI.", "TG_GSQL_SERVER_URL", TG_GSQL_SERVER_URL);
                return;
            }
            else
            {
                /* Setup authentication to the TG servers */
                RestServerWebClient = new WebClient();
                RestServerWebClient.Headers.Add("Authorization", "Bearer " + TG_TOKEN);
                GsqlServerWebClient = new WebClient();
                string credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(TG_USER + ":" + TG_PASS));
                GsqlServerWebClient.Headers[HttpRequestHeader.Authorization] = string.Format(
                    "Basic {0}", credentials);
                Log.Information("Authentication to REST++ server at {0} and GSQL server at {1} initialized.", TG_REST_SERVER_URL, TG_GSQL_SERVER_URL);
                /* Setup the timer for pinging the server.*/
                var pingTimer = new System.Timers.Timer(1000 * 60 * 15);
                pingTimer.Elapsed += (sender, e) => {
                    var now = DateTime.Now;
                    try
                    {
                        RestServerWebClient.DownloadString(rsu.ToString() + "echo");
                        Log.Information("Pinged {0} at {1}.", rsu, now);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error attempting to ping {0} at {1}.", rsu, now);
                    }
                };
                pingTimer.Enabled = true;
                CreateHostBuilder(args).Build().Run();
                /* After the webhost shuts down dispose of any unmanaged resources. */
                RestServerWebClient.Dispose();
                GsqlServerWebClient.Dispose();
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

        public static WebClient RestServerWebClient = new WebClient();

        public static WebClient GsqlServerWebClient = new WebClient();
        #endregion

        #region Methods
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
        #endregion
    }
}
