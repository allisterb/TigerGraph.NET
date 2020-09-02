using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using RestSharp;

using TigerGraph.Models;

namespace TigerGraph
{
    public class ApiClient : Base.ApiClient
    {
        #region Constructors
        static ApiClient()
        {
            Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();
        }
        public ApiClient(string token, Uri restServerUrl, Uri gsqlServerUrl) : base(token, restServerUrl, gsqlServerUrl)
        {
            RestClient = new RestClient(RestServerUrl);
            RestClient.AddDefaultHeader("Authorization", "Bearer " + Token);
            Info("Initialized REST++ client authentication for server URL {0}.", RestServerUrl);
            GsqlClient = new RestClient(GsqlServerUrl);
            GsqlClient.AddDefaultHeader("Authorization", "Bearer " + Token);
            Info("Initialized GSQL client authentication for server URL {0}.", GsqlServerUrl);
            Initialized = true;
        }

        public ApiClient() :  this(Config("TG_Token"), new Uri(Config("TG_REST_SERVER_URL")), new Uri(Config("TG_GSQL_SERVER_URL"))) {}
        #endregion

        #region Implemented members
        public override async Task<T> RestHttpGetAsync<T>(string query)
        {
            var request = new RestRequest("echo", Method.GET);
            IRestResponse response = await RestClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
        }

        public override Task<T> RestGsqlGetAsync<T>(string query)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Properties
        public static IConfigurationRoot Configuration { get; protected set; }

        public static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;

        public static System.Version AssemblyVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;

        public static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static string Config(string i) => Configuration[i];

        public RestClient RestClient { get; set; }

        public RestClient GsqlClient { get; set; }
        #endregion

        #region Methods

        #endregion
    }
}
