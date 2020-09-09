using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
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
            if (!string.IsNullOrEmpty(token))
            {
                RestClient.AddDefaultHeader("Authorization", "Bearer " + Token);
                Info("Initialized REST++ client authentication using specified token.");
            }
            GsqlClient = new RestClient(GsqlServerUrl);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                GsqlClient.Authenticator = new RestSharp.Authenticators.HttpBasicAuthenticator(User, Pass);
                Info("Initialized GSQL client authentication using specified name and password.");
            }
            Initialized = true;
        }

        public ApiClient() :  this(Config("TG_Token"), GetUri(Config("TG_REST_SERVER_URL")), GetUri(Config("TG_GSQL_SERVER_URL")), Config("TG_USER"), Config("TG_PASS")) {}
        #endregion

        #region Implemented members
        public override async Task<T> RestHttpGetAsync<T>(string query)
        {
            var request = new RestRequest(query, Method.GET);
            Debug("HTTP GET:{0}", RestServerUrl.ToString() + query);
            IRestResponse response = await RestClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                Debug("JSON response: {0}", response.Content);
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
        }

        public override async Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data)
        {
            var request = new RestRequest(query, Method.POST, DataFormat.Json);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(data), ParameterType.RequestBody);
            Debug("HTTP POST:{0}", RestServerUrl.ToString() + query);
            IRestResponse response = await RestClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                Debug("JSON response: {0}", response.Content);
                return JsonConvert.DeserializeObject<T2>(response.Content);
            }
        }

        public override async Task<T> GsqlHttpGetAsync<T>(string query)
        {
            var request = new RestRequest(query, Method.GET);
            Debug("HTTP query:{0}", GsqlServerUrl.ToString() + query);
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
                Debug("JSON response: {0}", response.Content);
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
        }

        public override async Task<T2> GsqlHttpPostAsync<T1, T2>(string query, T1 data)
        {
            var request = new RestRequest(query, Method.POST, DataFormat.Json);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(data), ParameterType.RequestBody);
            Debug("HTTP POST:{0}", GsqlServerUrl.ToString() + query);
            IRestResponse response = await GsqlClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                Debug("JSON response: {0}", response.Content);
                return JsonConvert.DeserializeObject<T2>(response.Content);
            }
        }

        public override async Task<T> GsqlHttpPostStringAsync<T>(string query, string data)
        {
            var request = new RestRequest(query, Method.POST, DataFormat.None);
            request.AddParameter("text/plain", data, ParameterType.RequestBody);
            Debug("HTTP POST:{0}", GsqlServerUrl.ToString() + query);
            IRestResponse response = await GsqlClient.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else
            {
                Debug("JSON response: {0}", response.Content);
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
