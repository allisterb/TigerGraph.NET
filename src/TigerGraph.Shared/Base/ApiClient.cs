using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TigerGraph.Models;

namespace TigerGraph.Base
{
    public abstract class ApiClient : Runtime, IApiClient
    {
        #region Constructors
        public ApiClient(string token, Uri restServerUrl, Uri gsqlServerUrl, string user, string pass) : base()
        {
            Token = token ?? throw new ArgumentException("Could not get the TigerGraph access token.");
            RestServerUrl = restServerUrl ?? throw new ArgumentException("Could not get the TigerGraph REST++ server URL.");
            GsqlServerUrl = gsqlServerUrl ?? throw new ArgumentException("Could not get the TigerGraph GSQL server URL.");
            User = user ?? throw new ArgumentException("Could not get the TigerGraph user name.");
            Pass = pass ?? throw new ArgumentException("Could not get the TigerGraph user password.");
            Info("Initialized REST++ client for {0} and GSQL client for {1}.", RestServerUrl, GsqlServerUrl);
        }
        #endregion

        #region Abstract members
        public abstract Task<T> RestHttpGetAsync<T>(string query);

        public abstract Task<T> GsqlHttpGetAsync<T>(string query);
        #endregion

        #region Properties
        public string Token { get; }

        public Uri RestServerUrl { get; set; }

        public Uri GsqlServerUrl { get; set; }
        
        public string User { get; set; }

        public string Pass { get; set; }
        #endregion

        #region Methods
        public async Task<EchoResponse> Echo()
        {
            FailIfNotInitialized();
            using (var op = Begin("Ping server {0}", RestServerUrl))
            {
                var response = await RestHttpGetAsync<EchoResponse>("echo");
                op.Complete();
                return response;
            }
        }

        public async Task<Dictionary<string, EndPointParameter>> Endpoints()
        {
            FailIfNotInitialized();
            using (var op = Begin("Get endpoints from server {0}", RestServerUrl))
            {
                var response = await RestHttpGetAsync<Dictionary<string, EndPointParameter>>("endpoints?builtin=true&dynamic=true&static=true");
                op.Complete();
                return response;
            }
        }

        public async Task<SchemaResult> Schema(string graphName, string vertexType = "", string edgeType="")
        {
            FailIfNotInitialized();
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, RestServerUrl))
            {
                var query = "gsqlserver/gsql/schema/?graph=" + graphName;
                if (!string.IsNullOrEmpty(vertexType))
                {
                    query += "&type=" + vertexType;
                }
                else if (!string.IsNullOrEmpty(edgeType))
                {
                    query += "type=" + edgeType;
                }
                var response = await GsqlHttpGetAsync<SchemaResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<VertexSchema> VertexSchema(string graphName, string vertexType = "")
        {
            FailIfNotInitialized();
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, RestServerUrl))
            {
                var query = "gsqlserver/gsql/schema/?graph=" + graphName + "&type=" + (vertexType ?? throw new ArgumentException("The vertex type parameter cannot be null."));
                var response = await GsqlHttpGetAsync<VertexSchema>(query);
                op.Complete();
                return response;
            }
        }

        #endregion
    }
}