using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using CO = Colorful.Console;
using Figgle;
using CommandLine;
using CommandLine.Text;

using TigerGraph.Models;

namespace TigerGraph.CLI
{
    #region Enums
    public enum ExitResult
    {
        SUCCESS = 0,
        UNHANDLED_EXCEPTION = 1,
        INVALID_OPTIONS = 2,
        NOT_FOUND = 4,
        SERVER_ERROR = 5,
        ERROR_IN_RESULTS = 6,
        UNKNOWN_ERROR = 7
    }
    #endregion
    class Program : Base.Runtime
    {
        #region Constructor
        static Program()
        {
            foreach (var t in OptionTypes)
            {
                OptionTypesMap.Add(t.Name, t);
            }
        }
        #endregion

        #region Entry point
        static void Main(string[] args)
        {
            if (args.Contains("--debug"))
            {
                SetLogger(new SerilogLogger(console: true, debug: true));
                Info("Debug mode set.");
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;   
                SetLogger(new SerilogLogger(console: true, debug: false));
            }
            PrintLogo();
#if WINDOWS && NET461
            ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, PingOptions, EndpointsOptions, SchemaOptions, VerticesOptions, EdgesOptions, UpsertOptions, QueryOptions, WinEvtOptions>(args);
#else
            ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, PingOptions, EndpointsOptions, SchemaOptions, VerticesOptions, EdgesOptions, UpsertOptions, QueryOptions>(args);
#endif
            result.WithParsed<ApiOptions>(o =>
            {
                ApiClient = new ApiClient(GetToken(o), GetRestServerUrl(o), GetGsqlServerUrl(o), GetUser(o), GetPass(o));
            })
            .WithParsed<PingOptions>(o =>
            {
                Exit(Echo(o).Result);
            })
            .WithParsed<EndpointsOptions>(o =>
            {
                Exit(Endpoints(o).Result);
            })
            .WithParsed<SchemaOptions>(o =>
            {
                Exit(Schema(o).Result);
            })
            .WithParsed<VerticesOptions>(o =>
            {
                Exit(Vertices(o).Result);
            })
            .WithParsed<EdgesOptions>(o =>
            {
                Exit(Edges(o).Result);
            })
#if WINDOWS && NET461
            .WithParsed<WinEvtOptions>(o =>
            {
                Exit(WinEvt(o).Result);
            })
#endif
            .WithParsed<UpsertOptions>(o =>
            {
                Exit(Upsert(o).Result);
            })
            .WithParsed<QueryOptions>(o =>
            {
                Exit(Query(o).Result);
            })
            #region Print options help
            .WithNotParsed((IEnumerable<Error> errors) =>
            {
                HelpText help = GetAutoBuiltHelpText(result);
                help.Copyright = "";
                if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
                {
                    help.Heading = new HeadingInfo("TigerGraph.NET", ApiClient.AssemblyVersion.ToString(3));
                    help.Copyright = new CopyrightInfo("Allister Beharry", new int[] { 2020 });
                    Info(help);
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.HelpVerbRequestedError))
                {
                    HelpVerbRequestedError error = (HelpVerbRequestedError)errors.First(e => e.Tag == ErrorType.HelpVerbRequestedError);
                    if (error.Type != null)
                    {
                        help.AddVerbs(error.Type);
                    }
                    else
                    {
                        help.AddVerbs(OptionTypes);
                    }
                    Info(help);
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.HelpRequestedError))
                {
                    HelpRequestedError error = (HelpRequestedError)errors.First(e => e.Tag == ErrorType.HelpRequestedError);
                    help.AddVerbs(result.TypeInfo.Current);
                    help.AddOptions(result);
                    help.AddPreOptionsLine($"{result.TypeInfo.Current.Name.Replace("Options", "").ToLower()} options:");
                    Info(help);
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
                {
                    help.AddVerbs(OptionTypes);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.MissingRequiredOptionError))
                {
                    MissingRequiredOptionError error = (MissingRequiredOptionError)errors.First(e => e.Tag == ErrorType.MissingRequiredOptionError);
                    Error("A required option is missing: {0}.", error.NameInfo.NameText);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.UnknownOptionError))
                {
                    UnknownOptionError error = (UnknownOptionError)errors.First(e => e.Tag == ErrorType.UnknownOptionError);
                    help.AddVerbs(OptionTypes);
                    Error("Unknown option: {error}.", error.Token);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else
                {
                    Error("An error occurred parsing the program options: {errors}.", errors);
                    help.AddVerbs(OptionTypes);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
            });
            #endregion
        }
        #endregion

        #region Methods
        static async Task<ExitResult> Echo(PingOptions o)
        {
            var r = await ApiClient.Echo();
            Info("Echo response: {0}", JsonConvert.SerializeObject(r).ToString());
            return ExitResult.SUCCESS;
        }

        static async Task<ExitResult> Endpoints(EndpointsOptions o)
        {
            var r = await ApiClient.Endpoints();
            Info("Received {0} endpoints from {1}:", r.Count, GetRestServerUrl(o));
            foreach(var e in r)
            {
                WriteInfo("{0}", e.Key);
            }
            return ExitResult.SUCCESS;
        }

        static async Task<ExitResult> Schema(SchemaOptions o)
        {
            if (string.IsNullOrEmpty(o.Vertex) && string.IsNullOrEmpty(o.Edge))
            {
                var r = await ApiClient.Schema(o.Graph);
                if (!r.error)
                {
                    Info("Received {0} vertex types and {1} edge types for graph {2} from {3}.", r.results.VertexTypes.Length, r.results.EdgeTypes.Length, o.Graph, GetGsqlServerUrl(o));
                    Info("Vertices: {0}", r.results.VertexTypes.Select(v => v.Name));
                    Info("Edges: {0}", r.results.EdgeTypes.Select(v => v.Name));
                }
                else
                {
                    Error("Error occurred retrieving schema for graph {0} from {1}: {2}.", o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
            else if (!string.IsNullOrEmpty(o.Vertex))
            {
                var r = await ApiClient.VertexSchema(o.Graph, o.Vertex);
                if (!r.error)
                {
                    Info("Vertex {0} has schema:\n{1}}", o.Vertex, JsonConvert.SerializeObject(r.results));
                }
                else
                {
                    Error("Error occurred retrieving vertex schema {0} in graph {1} from {2}: {3}.", o.Vertex, o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
            else if (!string.IsNullOrEmpty(o.Edge))
            {
                var r = await ApiClient.EdgeSchema(o.Graph, o.Edge);
                if (!r.error)
                {
                    Info("Edge {0} has schema:\n{1}}", o.Edge, JsonConvert.SerializeObject(r.results));
                }
                else
                {
                    Error("Error occurred retrieving edge schema {0} in graph {1} from {2}: {3}.", o.Edge, o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
            else
            {
                Error("You can only retrieve a schema for a single vertex or edge at a time.");
                return ExitResult.INVALID_OPTIONS;
            }
        }

        static async Task<ExitResult> Vertices(VerticesOptions o)
        {
            if (!o.Count)
            {
                var r = await ApiClient.Vertices(o.Graph, o.Vertex, o.Id);
                if (!r.error)
                {
                    if (!string.IsNullOrEmpty(o.Id))
                    {
                        Info("{0} vertex with id {1}:\n{2}}", o.Vertex, o.Id, JsonConvert.SerializeObject(r.results));
                    }
                    else
                    {
                        Info("{0} vertices:\n{1}", o.Vertex, JsonConvert.SerializeObject(r.results));
                    }
                }
                else
                {
                    Error("Error occurred retrieving {0} vertex data in graph {1} from {2}: {3}.", o.Vertex, o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
            else if (!string.IsNullOrEmpty(o.Id))
            {
                Error("You cannot specify the vertex id and also count the number of vertices.");
                return ExitResult.INVALID_OPTIONS;
           
            }
            else
            {
                var r = await ApiClient.VerticesCount(o.Graph, o.Vertex);
                if (!r.error)
                {
                    Info("{0} vertices: {1}.", o.Vertex, r.results.Select(c => c.count));
                    return ExitResult.SUCCESS;
                }
                else
                {
                    Error("Error retrieving {0} vertex count for graph {1}: {2}.", o.Vertex, o.Graph, r.message);
                    return ExitResult.ERROR_IN_RESULTS;
                }
            }
        }

        static async Task<ExitResult> Edges(EdgesOptions o)
        {
            var r = await ApiClient.Edges(o.Graph, o.Source, o.Id, o.Target, o.Tid, o.Edge, o.Count);
            if (!o.Count)
            {
                if (!r.error)
                {
                    if (string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                    {
                        Info("All edges from source {0} vertex with id {1}:\n{2}", o.Source, o.Id, JsonConvert.SerializeObject(r.results));
                    }
                    else if (!string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                    {
                        Info("{0} edges from source {1} vertex with id {2}:\n{3}", o.Edge, o.Source, o.Id, JsonConvert.SerializeObject(r.results));
                    }
                    else if (string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                    {
                        Info("All edges from source {0} vertex with id {1} to target {2} vertex with id {3}:\n{4}", o.Source, o.Id, o.Target, o.Tid, JsonConvert.SerializeObject(r.results));
                    }
                    else if (!string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                    {
                        Info("{0} edges from source {1} vertex with id {2} to target {3} vertex with id {4}:\n{5}", o.Edge, o.Source, o.Id, o.Target, o.Tid, JsonConvert.SerializeObject(r.results));
                    }
                    else throw new InvalidOperationException("Unsupported CLI options combination.");
                }
                else
                {
                    Error("Error occurred retrieving {0} edge data in graph {1} from {2}: {3}.", o.Edge, o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
            else
            {
                if (!r.error)
                {
                    if (string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                    {
                        Info("Count of all edges from source {0} vertex with id {1}:\n{2}", o.Source, o.Id, r.results.Where(e => e.count.Value > 0).Select(e => (e.e_type, e.from_id, e.from_type, e.count)));
                    }
                    else if (!string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                    {
                        Info("Count of {0} edges from source {1} vertex with id {2}:\n{3}", o.Edge, o.Source, o.Id, r.results.Where(e => e.count.Value > 0).Select(e => (e.e_type, e.from_id, e.from_type, e.count)));
                    }
                    else if (string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                    {
                        Info("Count edges from source {0} vertex with id {1} to target {2} vertex with id {3}:\n{4}", o.Source, o.Id, o.Target, o.Tid, r.results.Where(e => e.count.Value > 0).Select(e => (e.e_type, e.from_id, e.from_type, e.count)));
                    }
                    else if (!string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                    {
                        Info("Count {0} edges from source {1} vertex with id {2} to target {3} vertex with id {4}:\n{5}", o.Edge, o.Source, o.Id, o.Target, o.Tid, r.results.Where(e => e.count.Value > 0).Select(e => (e.e_type, e.from_id, e.from_type, e.count)));
                    }
                    else throw new InvalidOperationException("Unsupported CLI options combination.");
                }
                else
                {
                    Error("Error occurred retrieving {0} edge data in graph {1} from {2}: {3}.", o.Edge, o.Graph, o.GsqlServerUrl, r.message);
                }
                return ExitResult.SUCCESS;
            }
        }

#if WINDOWS && NET461
        static async Task<ExitResult> WinEvt(WinEvtOptions o)
        {
            var path = Environment.ExpandEnvironmentVariables(SysMonEvtLogPath);
            if (!File.Exists(path))
            {
                Error("The SysMon event log file at {0} was not found. Ensure you have installed SysMon on this system.", path);
                return ExitResult.NOT_FOUND;
            }
            else
            {
                EventData = new Upsert();
                var winevt = new WinEvtLogReader(EventData);
                Info("Using SysMon event log file at {0}.", path);
                Info("Reading SysMon event log. Press any key to exit.");
                winevt.ReadSysMonLog();
                Info("Read {0} and {1} vertices and edges from SysMon event log.", EventData.vertices.Count(), EventData.edges.Count());
                return ExitResult.SUCCESS;
            }
        }
#endif
        static async Task<ExitResult> Upsert(UpsertOptions o)
        {
            if (!File.Exists(o.File))
            {
                Error("Could not find the file {0}.", o.File);
                return ExitResult.INVALID_OPTIONS;
            }
            Upsert data;
            using (var op = Begin("Reading JSON data from file {0}", o.File))
            {
                data = JsonConvert.DeserializeObject<Upsert>(File.ReadAllText(o.File));
                op.Complete();
            }
            var r = await ApiClient.Upsert(o.Graph, data, true, false, false);
            if (!r.error)
            {
                for (int i = 0; i < r.results.Count(); i++)
                {
                    Info("Successfully upserted {0} {1} vertices to graph {2} at {3}.", r.results[i].accepted_vertices, data.vertices.Keys.ElementAt(i), o.Graph, GetRestServerUrl(o));
                }
                return ExitResult.SUCCESS;
            }
            else
            {
                Error("Failed to upsert data: {0} ({1}).", r.message, r.code);
                return ExitResult.ERROR_IN_RESULTS;
            }

        }

        static async Task<ExitResult> Query(QueryOptions o)
        {
            var source = "";
            Dictionary<string, object> parsed_params = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(o.Source) && string.IsNullOrEmpty(o.File))
            {
                Error("You must specify either the source of the query using {0} or the file containing the query source using {1}.", "-s", "-f");
                return ExitResult.INVALID_OPTIONS;

            }
            else if (!string.IsNullOrEmpty(o.Source) && !string.IsNullOrEmpty(o.File))
            {
                Error("You cannot specify both the -f and -t options for the query.");
                return ExitResult.INVALID_OPTIONS;
            }
            else if (string.IsNullOrEmpty(o.Source) && !string.IsNullOrEmpty(o.File))
            {
                if (!File.Exists(o.File))
                {
                    Error("Could not find the file {0}.", o.File);
                    return ExitResult.INVALID_OPTIONS;
                }
                else
                {
                    source = File.ReadAllText(o.File);
                }
            }
            else
            {
                source = o.Source;
            }
            if (!string.IsNullOrEmpty(o.Parameters))
            {
                parsed_params = Options.Parse(o.Parameters);

                if (parsed_params.Count == 0)
                {
                    Error("There was an error parsing the query parameters {0}.", o.Parameters);
                    return ExitResult.INVALID_OPTIONS;
                }
                else if (parsed_params.Where(p => p.Key == "_ERROR_").Count() > 0)
                {
                    string error_params = parsed_params.Where(p => p.Key == "_ERROR_").Select(kv => (string)kv.Value).Aggregate((s1, s2) => s1 + Environment.NewLine + s2);
                    Error("There was an error parsing the following options {0}.", error_params);
                    parsed_params = parsed_params.Where(p => p.Key != "_ERROR_").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            using (var op = Begin("Executing query {0} on server {1}", source, GetGsqlServerUrl(o)))
            {
                var r = await ApiClient.Query(source, parsed_params);
                op.Complete();
                if (!r.error)
                {
                    Info("Query results: {0}.", r.results);
                    return ExitResult.SUCCESS;
                }
                else
                {
                    Error("The query returned an error: {0}", r.message);
                    return ExitResult.ERROR_IN_RESULTS;
                }
            }
        }
        #region Get parameters
        static string GetToken(ApiOptions o)
        {
            var token = string.IsNullOrEmpty(o.Token) ? Environment.GetEnvironmentVariable("TG_TOKEN") : o.Token;
            if (string.IsNullOrEmpty(token))
            {
                Error("The token parameter was not specified and the environment variable TG_TOKEN does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
            }
            Debug("Token is: {0}.", token);
            return token;
        }

        static string GetUser(ApiOptions o)
        {
            var user = string.IsNullOrEmpty(o.User) ? Environment.GetEnvironmentVariable("TG_USER") : o.User;
            if (string.IsNullOrEmpty(user))
            {
                Error("The user parameter was not specified and the environment variable TG_USER does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
            }
            Debug("User is: {0}.", user);
            return user;
        }

        static string GetPass(ApiOptions o)
        {
            var pass = string.IsNullOrEmpty(o.GsqlServerUrl) ? Environment.GetEnvironmentVariable("TG_PASS") : o.Pass;
            if (string.IsNullOrEmpty(pass))
            {
                Error("The user password parameter was not specified and the environment variable TG_PASS does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
            }
            Debug("Pass is: {0}.", pass);
            return pass;
        }

        static Uri GetRestServerUrl(ApiOptions o)
        {
            var url = string.IsNullOrEmpty(o.RestServerUrl) ? Environment.GetEnvironmentVariable("TG_REST_SERVER_URL") : o.RestServerUrl;
            if (string.IsNullOrEmpty(url))
            {
                Error("The REST++ server URL parameter was not specified and the environment variable TG_REST_SERVER_URL does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
                return null;
            }
            else if (!Uri.TryCreate(url, UriKind.Absolute, out Uri u))
            {
                Error("The REST++ server URL parameter is not a valid URI: {0}.", url);
                Exit(ExitResult.INVALID_OPTIONS);
                return null;
            }
            else
            {
                Debug("Rest server url is: {0}.", u);
                return u;
            }
        }

        static Uri GetGsqlServerUrl(ApiOptions o)
        {
            var gurl = string.IsNullOrEmpty(o.GsqlServerUrl) ? Environment.GetEnvironmentVariable("TG_GSQL_SERVER_URL") : o.GsqlServerUrl;
            if (string.IsNullOrEmpty(gurl))
            {
                Error("The GSQL server URL parameter was not specified and the environment variable TG_GSQL_SERVER_URL does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
                return null;
            }
            else if (!Uri.TryCreate(gurl, UriKind.Absolute, out Uri u))
            {
                Error("The GSQL server URL parameter is not a valid URI: {0}.", gurl);
                Exit(ExitResult.INVALID_OPTIONS);
                return null;
            }
            else
            {
                Debug("GSQL server url is: {0}.", u);
                return u;
            }
        }
        #endregion

        static void PrintLogo()
        {
            CO.WriteLine(FiggleFonts.Chunky.Render("TigerGraph.NET"), Color.Blue);
            CO.WriteLine("v{0}", ApiClient.AssemblyVersion.ToString(3), Color.Blue);
        }

        public static void Exit(ExitResult result)
        {

            if (Cts != null && !Cts.Token.CanBeCanceled)
            {
                Cts.Cancel();
                Cts.Dispose();
            }
            Environment.Exit((int)result);
        }

        static HelpText GetAutoBuiltHelpText(ParserResult<object> result)
        {
            return HelpText.AutoBuild(result, h =>
            {
                h.AddOptions(result);
                return h;
            },
            e =>
            {
                return e;
            });
        }

        static void WriteInfo(string template, params object[] args) => CO.WriteLineFormatted(template, Color.AliceBlue, Color.PaleGoldenrod, args);

        #endregion

        #region Properties
        public static string SysMonEvtLogPath { get; } = "%SystemRoot%\\System32\\Winevt\\Logs\\Microsoft-Windows-Sysmon%4Operational.evtx";

#if WINDOWS && NET461
        static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(PingOptions), typeof(EndpointsOptions),
            typeof(SchemaOptions), typeof(VerticesOptions), typeof(EdgesOptions), typeof(UpsertOptions), typeof(QueryOptions), typeof(WinEvtOptions) };
#else
        static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(PingOptions), typeof(EndpointsOptions), 
            typeof(SchemaOptions), typeof(VerticesOptions), typeof(EdgesOptions), typeof(UpsertOptions), typeof(QueryOptions)};
#endif

        static Dictionary<string, Type> OptionTypesMap { get; } = new Dictionary<string, Type>();
        static ApiClient ApiClient { get; set; }

        static Upsert EventData {get; set;}
        #endregion

        #region Event Handlers
        private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Error((Exception)e.ExceptionObject, "Unhandled runtime error occurred. TigerGraph.NET CLI will now shutdown.");
            Exit(ExitResult.UNHANDLED_EXCEPTION);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Info("Ctrl-C pressed. Exiting.");
            Cts.Cancel();
            Exit(ExitResult.SUCCESS);
        }
        #endregion
    }
}
