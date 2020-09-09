using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CommandLine;
using CommandLine.Text;

namespace TigerGraph.CLI
{
    public class Options
    {
        [Option('d', "debug", Required = false, HelpText = "Enable debug mode.")]
        public bool Debug { get; set; }

        public static Dictionary<string, object> Parse(string o)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            Regex re = new Regex(@"(\w+)\=([^\,]+)", RegexOptions.Compiled);
            string[] pairs = o.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in pairs)
            {
                Match m = re.Match(s);
                if (!m.Success)
                {
                    options.Add("_ERROR_", s);
                }
                else if (options.ContainsKey(m.Groups[1].Value))
                {
                    options[m.Groups[1].Value] = m.Groups[2].Value;
                }
                else
                {
                    options.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }
            return options;
        }
    }

    public class ApiOptions : Options
    {
        [Option('t', "token", Required = false, HelpText = "Your TigerGraph server instance access token. If none is specified then use the environment variable TG_TOKEN.")]
        public string Token { get; set; }

        [Option('r', "rest", Required = false, HelpText = "Your TigerGraph REST++ server instance URL. If none is specified then use the environment variable TG_REST_SERVER_URL.")]
        public string RestServerUrl { get; set; }

        [Option('q', "gsql", Required = false, HelpText = "Your TigerGraph GSQL server instance URL. If none is specified then use the environment variable TG_GSQL_SERVER_URL.")]
        public string GsqlServerUrl { get; set; }

        [Option('u', "user", Required = false, HelpText = "Your TigerGraph server user name. If none is specified then use the environment variable TG_USR.")]
        public string User { get; set; }

        [Option("pass", Required = false, HelpText = "Your TigerGraph server user password. If none is specified then use the environment variable TG_PASS.")]
        public string Pass { get; set; }
    }

    [Verb("ping", HelpText = "Ping a TigerGraph server instance using the specified server URL and access token.")]
    public class PingOptions : ApiOptions {}

    [Verb("endpoints", HelpText = "Get the endpoints of the specified server.")]
    public class EndpointsOptions : ApiOptions {}

    [Verb("schema", HelpText = "Get the schema of the specified graph or of a specified vertex or edge type.")]
    public class SchemaOptions : ApiOptions 
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string Graph { get; set; }

        [Option('v', "vertex", Required = false, HelpText = "The vertex type to retrieve the schema for.")]
        public string Vertex { get; set; }

        [Option('e', "edge", Required = false, HelpText = "The edge type to retrieve the schema for.")]
        public string Edge { get; set; }
    }

    [Verb("vertices", HelpText = "Get the vertices of the specified graph of a specified vertex type and optionally with a specified vertex id.")]
    public class VerticesOptions : ApiOptions
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string Graph { get; set; }

        [Option('v', "vertex", Required = true, HelpText = "The vertex type to retrieve the data for.")]
        public string Vertex { get; set; }

        [Option('i', "id", Required = false, HelpText = "A specific vertex or edge id to retrieve.")]
        public string Id { get; set; }
    }

    [Verb("edges", HelpText = "Get the edges of the specified graph by specifying the source vertices and optionally the edge type and target vertices.")]
    public class EdgesOptions : ApiOptions
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph. Defaults to 'MyGraph'.")]
        public string Graph { get; set; }

        [Option('v', "vertex", Required = true, HelpText = "The source vertex type.")]
        public string Source { get; set; }

        [Option("id", Required = true, HelpText = "An optional source vertex id.")]
        public string Id { get; set; }
        
        [Option('e', "edge", Required = false, HelpText = "An optional edge type to retrieve the edge data for.")]
        public string Edge { get; set; }

        [Option('t', "target", Required = false, HelpText = "The optional target vertex type.")]
        public string Target { get; set; }

        [Option("tid", Required = false, HelpText = "The optional target vertex id.")]
        public string Tid { get; set; }
    }
    
    #if WINDOWS && NET461
    [Verb("winevt", HelpText = "Ingest Windows event log data.")]
    public class WinEvtOptions : ApiOptions
    {
        [Option("sysmon", Required = false, HelpText = "Ingest event log data from the SysMon program.")]
        public bool SysMon { get; set; }
    }
    #endif
    
    [Verb("upsert", HelpText = "Upsert vertices and edges into graph.")]
    public class UpsertOptions : ApiOptions
    {
        [Option('g', "graph", Required = false, Default = "MyGraph", HelpText = "The name of the graph.")]
        public string Graph { get; set; }

        [Option('f', "file", Required = true, HelpText = "File containing the JSON data to upsert.")]
        public string File { get; set; }
    }

    [Verb("query", HelpText = "Execute a GSQL query on the specified graph using the specified parameters.")]
    public class QueryOptions : ApiOptions
    {
        [Option('s', "source", Required = false, HelpText = "The GSQL query to run.")]
        public string Source { get; set; }

        [Option('f', "file", Required = false, HelpText = "A file containing the GSQL query to run.")]
        public string File { get; set; }

        [Option('p', "params", Required = false, HelpText = "A comma-delimited list of query parameters in the form p1=v1,p2=v2,...")]
        public string Parameters { get; set; }
    }
}
