﻿using System;
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

        public async Task<SchemaResult> Schema(string graphName)
        {
            FailIfNotInitialized();
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, RestServerUrl))
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
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, RestServerUrl))
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
            using (var op = Begin("Get schema for graph {0} from server {1}", graphName, RestServerUrl))
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
            using (var op = Begin("Get {0} vertices for graph {1} from server {2}", vertexType, graphName, RestServerUrl))
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

        public async Task<EdgesResult> Edges(string graphName, string sourceVertexType, string sourceVertexId, string targetVertexType = "", string targetVertexId = "", string edgeType = "")
        {
            FailIfNotInitialized();
            if (targetVertexType == "" && targetVertexId == "" && edgeType == "")
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
            else if (targetVertexType != "" && targetVertexId != "" && edgeType == "")
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
            else if (targetVertexType != "" && targetVertexId != "" && edgeType != "")
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
            else throw new ArgumentException("Unsupported arguments.");
        }
        #endregion
    }
}