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
        public ApiClient(string token, Uri restServerUrl, Uri gsqlServerUrl, string user, string pass) : base(token, restServerUrl, gsqlServerUrl, user, pass)
        {
            RestClient = new RestClient(RestServerUrl);
            RestClient.AddDefaultHeader("Authorization", "Bearer " + Token);
            Info("Initialized REST++ client authentication.");
            GsqlClient = new RestClient(GsqlServerUrl);
            GsqlClient.Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator(User, Pass);
            Info("Initialized GSQL client authentication.");
            Initialized = true;
        }

        public ApiClient() :  this(Config("TG_Token"), GetUri(Config("TG_REST_SERVER_URL")), GetUri(Config("TG_GSQL_SERVER_URL")), Config("TG_USER"), Config("TG_PASS")) {}
        #endregion

        #region Implemented members
        public override async Task<T> RestHttpGetAsync<T>(string query)
        {
            var request = new RestRequest(query, Method.GET);
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

        public override async Task<T> GsqlHttpGetAsync<T>(string query)
        {
            var request = new RestRequest(query, Method.GET);
            IRestResponse response = await GsqlClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (!response.IsSuccessful)
            {
                throw new Exception($"HTTP request to GSQL server at {GsqlServerUrl} was not successfule: {response.ErrorMessage}");
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
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
        private static Uri GetUri(string u)
        {
            if (!Uri.TryCreate(u, UriKind.Absolute, out Uri uri))
            {
                throw new ArgumentException($"The string {u} is not a valid URI.");
            }
            else return uri;
        }
        #endregion
    }
}
