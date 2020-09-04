using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using CO = Colorful.Console;
using Figgle;
using CommandLine;
using CommandLine.Text;

namespace TigerGraph.CLI
{
    #region Enums
    public enum ExitResult
    {
        SUCCESS = 0,
        UNHANDLED_EXCEPTION = 1,
        INVALID_OPTIONS = 2,
        UNKNOWN_ERROR = 3,
        NOT_FOUND_OR_SERVER_ERROR = 4
    }
    #endregion

    class Program : Base.Runtime
    {
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
                SetLogger(new SerilogLogger(console: true, debug: false));
            }
            PrintLogo();
#if WINDOWS && NET461
            ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, PingOptions, EndpointsOptions, SchemaOptions, VerticesOptions, EdgesOptions, WinEvtOptions>(args);
#else
            ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, PingOptions, EndpointsOptions, SchemaOptions, VerticesOptions, EdgesOptions>(args);
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

            #region Print options help
            .WithNotParsed((IEnumerable<Error> errors) =>
            {
                HelpText help = GetAutoBuiltHelpText(result);
                help.Copyright = string.Empty;
                help.AddPreOptionsLine(string.Empty);

                if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
                {
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
                    help.AddVerbs(OptionTypes);
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
            Info("Received {0} endpoints from {1}: {2}", r.Count, o.RestServerUrl, r.Keys);
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
            var r = await ApiClient.Vertices(o.Graph, o.Vertex, o.Id);
            if (!r.error)
            {
                if (!string.IsNullOrEmpty(o.Id))
                {
                    Info("{0} vertex with id {1}:\n{2}}", o.Vertex, o.Id, JsonConvert.SerializeObject(r.results));
                }
                else
                {
                    Info("{0} vertices:\n{1}}", o.Vertex, JsonConvert.SerializeObject(r.results));
                }
            }
            else
            {
                Error("Error occurred retrieving {0} vertex data in graph {1} from {2}: {3}.", o.Vertex, o.Graph, o.GsqlServerUrl, r.message);
            }
            return ExitResult.SUCCESS;
        }

        static async Task<ExitResult> Edges(EdgesOptions o)
        {
            var r = await ApiClient.Edges(o.Graph, o.Source, o.Id, o.Target, o.Tid, o.Edge);
            if (!r.error)
            {
                if (string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                {
                    Info("All edges from source {0} vertex with id {1}:\n{2}}", o.Source, o.Id, JsonConvert.SerializeObject(r.results));
                }
                else if (!string.IsNullOrEmpty(o.Edge) && string.IsNullOrEmpty(o.Target) && string.IsNullOrEmpty(o.Tid))
                {
                    Info("{0} Edges from source {1} vertex with id {2}:\n{3}}", o.Edge, o.Source, o.Id, JsonConvert.SerializeObject(r.results));
                }
                else if (string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                {
                    Info("All edges from source {0} vertex with id {1} to target {2} vertex with id {3}:\n{4}}", o.Source, o.Id, o.Target, o.Tid, JsonConvert.SerializeObject(r.results));
                }
                else if (!string.IsNullOrEmpty(o.Edge) && !string.IsNullOrEmpty(o.Target) && !string.IsNullOrEmpty(o.Tid))
                {
                    Info("{0} edges from source {1} vertex with id {2} to target {3} vertex with id {4}:\n{5}}", o.Edge, o.Source, o.Id, o.Target, o.Tid, JsonConvert.SerializeObject(r.results));
                }
                else throw new InvalidOperationException("Unsupported CLI options combination.");
            }
            else
            {
                Error("Error occurred retrieving {0} edge data in graph {1} from {2}: {3}.", o.Edge, o.Graph, o.GsqlServerUrl, r.message);
            }
            return ExitResult.SUCCESS;
        }

#if WINDOWS && NET461
        static async Task<ExitResult> WinEvt(WinEvtOptions o)
        {
            var path = Environment.ExpandEnvironmentVariables(SysMonEvtLogPath);
            if (!File.Exists(path))
            {
                Error("The SysMon event log file at {0} was not found. Ensure you have installed SysMon on this system.", path);
                return ExitResult.NOT_FOUND_OR_SERVER_ERROR;
            }
            else
            {
                Info("Using SysMon event log file at {0}.", path);
            }
            return ExitResult.SUCCESS;
        }
        #endif
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
            CO.WriteLine(FiggleFonts.Chunky.Render("TigerGraph"), Color.Blue);
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
        static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(PingOptions), typeof(EndpointsOptions), typeof(SchemaOptions), typeof(VerticesOptions), typeof(EdgesOptions), typeof(WinEvtOptions) };
        #else
        static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(PingOptions), typeof(EndpointsOptions), typeof(SchemaOptions), typeof(VerticesOptions), typeof(EdgesOptions)};
        #endif
        static ApiClient ApiClient {get; set; }
        #endregion

        #region Event Handlers
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            Error((Exception)e.ExceptionObject, "Unhandled error occurred during operation. SMApp CLI will now shutdown.");
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
