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
            ParserResult<object> result = new Parser().ParseArguments<Options, ApiOptions, PingOptions, EndpointsOptions>(args);
            #region Print options help
            result.WithNotParsed((IEnumerable<Error> errors) =>
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
            })
            #endregion
            .WithParsed<ApiOptions>(o =>
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
            });
        }
        #endregion

        #region Methods
        static string GetToken(ApiOptions o)
        {
            var token = string.IsNullOrEmpty(o.Token) ? Environment.GetEnvironmentVariable("TG_TOKEN") : o.Token;
            if (string.IsNullOrEmpty(token))
            {
                Error("The token parameter was not specified and the environment variable TG_TOKEN does not exist or is empty.");
                Exit(ExitResult.INVALID_OPTIONS);
            }
            Debug("Token is: ", token);
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
            else return u;
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
            else return u;
        }

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
        static Type[] OptionTypes = { typeof(Options), typeof(ApiOptions), typeof(PingOptions), typeof(EndpointsOptions) };

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
