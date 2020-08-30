using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RestSharp;

using TigerGraph.Models;
namespace TigerGraph
{
    public class ApiClient : Runtime
    {
        #region Constructors
        public ApiClient(string token, Uri url) : base()
        {
            Token = token ?? throw new ArgumentException("Could not get the TigerGraph access token.");
            BaseUrl = url ?? throw new ArgumentException("Could not get the TigerGraph server URL.");
            RestClient = new RestClient(BaseUrl);
            RestClient.AddDefaultHeader("Authorization", Token);
            Info("Initialized REST++ client for server URL {0}.", BaseUrl);
            Initialized = true;
        }

        public ApiClient() :  this(Config("TG"), new Uri(Config("TG_URL"))) {}
        #endregion

        #region Properties
        public string Token { get; }

        public Uri BaseUrl { get; set; }

        public RestClient RestClient { get; set; }
        #endregion

        #region Methods
        public async Task<EchoResponse> Echo()
        {
            ThrowIfNotInitialized();
            using (var op = Begin("Echo"))
            {
                var request = new RestRequest("echo", Method.GET);
                IRestResponse response = await RestClient.ExecuteAsync(request);
                if (response.ErrorException != null) throw response.ErrorException;
                op.Complete();
                return JsonConvert.DeserializeObject<EchoResponse>(response.Content);
            }
        }
        #endregion
    }
}
