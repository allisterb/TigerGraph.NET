using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public abstract Task<T2> RestHttpPostAsync<T1, T2>(string query, T1 data);

        public abstract Task<T2> GsqlHttpPostAsync<T1, T2>(string query, T1 data);

        public abstract Task<T> GsqlHttpPostStringAsync<T>(string query, string data);
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

        public async Task<SchemaResult> Schema(string graphName)
        {
            FailIfNotInitialized();
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, GsqlServerUrl))
            {
                var query = "gsqlserver/gsql/schema/?graph=" + graphName;
                var response = await GsqlHttpGetAsync<SchemaResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<VertexSchemaResult> VertexSchema(string graphName, string vertexType)
        {
            FailIfNotInitialized();
            using (var op = Begin("Get vertex {0} schema for graph {1} from server {2}", vertexType, graphName, GsqlServerUrl))
            {
                var query = "gsqlserver/gsql/schema/?graph=" + graphName + "&type=" + (vertexType ?? throw new ArgumentException("The vertex type parameter cannot be null."));
                var response = await GsqlHttpGetAsync<VertexSchemaResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<EdgeSchemaResult> EdgeSchema(string graphName, string edgeType)
        {
            FailIfNotInitialized();
            using (var op = Begin("Get edge {0} schema for graph {1} from server {2}", edgeType, graphName, GsqlServerUrl))
            {
                var query = "gsqlserver/gsql/schema/?graph=" + graphName + "&type=" + (edgeType ?? throw new ArgumentException("The edge type parameter cannot be null."));
                var response = await GsqlHttpGetAsync<EdgeSchemaResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<VerticesResult> Vertices(string graphName, string vertexType, string vertexId = "")
        {
            FailIfNotInitialized();
            using (var op = Begin("Get {0} vertices with id {1} for graph {2} from server {3}", vertexType, string.IsNullOrEmpty(vertexId) ? "*" : vertexId, graphName, RestServerUrl))
            {
                var query = "graph/"+  graphName + "/vertices/" + (vertexType ?? throw new ArgumentException("The vertex type parameter cannot be null."));
                if (!string.IsNullOrEmpty(vertexId))
                {
                    query += "/" + vertexId;
                }
                var response = await RestHttpGetAsync<VerticesResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<VerticesCountResult> VerticesCount(string graphName, string vertexType)
        {
            FailIfNotInitialized();
            using (var op = Begin("Get count of {0} vertices for graph {1} from server {2}", vertexType, graphName, RestServerUrl))
            {
                var query = "graph/" + graphName + "/vertices/" + (vertexType ?? throw new ArgumentException("The vertex type parameter cannot be null."));
                query += "?count_only=true";
                var response = await RestHttpGetAsync<VerticesCountResult>(query);
                op.Complete();
                return response;
            }
        }

        public async Task<EdgesResult> Edges(string graphName, string sourceVertexType, string sourceVertexId, string targetVertexType = "", string targetVertexId = "", string edgeType = "")
        {
            FailIfNotInitialized();
            if (string.IsNullOrEmpty(targetVertexType) && string.IsNullOrEmpty(targetVertexId) && string.IsNullOrEmpty(edgeType))
            {
                using (var op = Begin("Get all edges for source {0} vertex with id {1} for graph {2} from server {3}", sourceVertexType, sourceVertexId, graphName, RestServerUrl))
                {
                    var query = "graph/" + graphName + "/edges/"
                        + (sourceVertexType ?? throw new ArgumentException("The source vertex type parameter cannot be null."))
                        + "/" + (sourceVertexId ?? throw new ArgumentException("The source vertex id parameter cannot be null."));
                    var response = await RestHttpGetAsync<EdgesResult>(query);
                    op.Complete();
                    return response;
                }
            }
            else if (!string.IsNullOrEmpty(targetVertexType) && !string.IsNullOrEmpty(targetVertexId) && string.IsNullOrEmpty(edgeType))
            {
                using (var op = Begin("Get all edges for source {0} vertex with id {1} to target {2} vertex with id {3} for graph {4} from server {5}", sourceVertexType, sourceVertexId, targetVertexType, targetVertexId, graphName, RestServerUrl))
                {
                    var query = "graph/" + graphName + "/edges/"
                        + (sourceVertexType ?? throw new ArgumentException("The source vertex type parameter cannot be null."))
                        + "/" + (sourceVertexId ?? throw new ArgumentException("The source vertex id parameter cannot be null."))
                        + "/_"
                        + "/" + (targetVertexType ?? throw new ArgumentException("The target vertex type parameter cannot be null."))
                        + "/" + (targetVertexId ?? throw new ArgumentException("The target vertex id parameter cannot be null."));
                    var response = await RestHttpGetAsync<EdgesResult>(query);
                    op.Complete();
                    return response;
                }
            }
            else if (!string.IsNullOrEmpty(targetVertexType) && !string.IsNullOrEmpty(targetVertexId) && !string.IsNullOrEmpty(edgeType))
            {
                using (var op = Begin("Get {0} edges for source {1} vertex with id {2} to target {3} vertex with id {4} for graph {5} from server {6}", edgeType, sourceVertexType, sourceVertexId, targetVertexType, targetVertexId, graphName, RestServerUrl))
                {
                    var query = "graph/" + graphName + "/edges"
                        + "/" + (sourceVertexType ?? throw new ArgumentException("The source vertex type parameter cannot be null."))
                        + "/" + (sourceVertexId ?? throw new ArgumentException("The source vertex id parameter cannot be null."))
                        + "/" + (edgeType ?? throw new ArgumentException("The edge type parameter cannot be null."))
                        + "/" + (targetVertexType ?? throw new ArgumentException("The target vertex type parameter cannot be null."))
                        + "/" + (targetVertexId ?? throw new ArgumentException("The target vertex id parameter cannot be null."));
                    var response = await RestHttpGetAsync<EdgesResult>(query);
                    op.Complete();
                    return response;
                }
            }
            else if (string.IsNullOrEmpty(targetVertexType) && string.IsNullOrEmpty(targetVertexId) && !string.IsNullOrEmpty(edgeType))
            {
                using (var op = Begin("Get {0} edges for source {1} vertex with id {2} to target {3} vertex with id {4} for graph {5} from server {6}", 
                    edgeType, sourceVertexType, sourceVertexId, targetVertexType, targetVertexId, graphName, RestServerUrl))
                {
                    var query = "graph/" + graphName + "/edges"
                        + "/" + (sourceVertexType ?? throw new ArgumentException("The source vertex type parameter cannot be null."))
                        + "/" + (sourceVertexId ?? throw new ArgumentException("The source vertex id parameter cannot be null."))
                        + "/" + (edgeType ?? throw new ArgumentException("The edge type parameter cannot be null."));
                    var response = await RestHttpGetAsync<EdgesResult>(query);
                    op.Complete();
                    return response;
                }
            }
            else throw new ArgumentException(string.Format("Unsupported arguments: Edge type:{0} Target Vertex Type: {1} Target Vertex ID:{2}", edgeType, targetVertexType, targetVertexId));
        }

        public async Task<UpsertResult> Upsert(string graphName, Upsert data, bool? ack, bool? new_vertex_only, bool? vertex_must_exist)
        {
            if (data.vertices == null)
            {
                data.vertices = new VerticesUpsert();
            }
            if (data.edges == null)
            {
                data.edges = new EdgesUpsert();
            }
            int vc = data.vertices.Count;
            int ec = data.edges.Count;
            using (var op = Begin("Upsert {0} vertices and {1} edges into graph {2} on server {3}", vc, ec, graphName, RestServerUrl))
            {
                var query = "graph/" + graphName;
                var response = await RestHttpPostAsync<Upsert, UpsertResult>(query, data);
                op.Complete();
                return response;
            }
        }

        public async Task<QueryResult> Query(string text, Dictionary<string, object> parameters)
        {
            var _p = "";
            foreach(var p in parameters)
            {
                _p += p.Key + "=" + p.Value.ToString() + "&";
            }
            _p = _p.TrimEnd('&');            
            var query = "gsqlserver/interpreted_query" + (parameters.Count > 0 ? "?" + _p : "");
            var response = await GsqlHttpPostStringAsync<QueryResult>(query, text);
            return response;
        }
        #endregion
    }
}