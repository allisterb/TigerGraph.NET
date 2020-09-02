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
            ParserResult<object> result = new Parser().ParseArguments<Options, RestApiOptions, PingOptions>(args);
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
            .WithParsed<RestApiOptions>(o => {
                var token = string.IsNullOrEmpty(o.Token) ? Environment.GetEnvironmentVariable("TG_TOKEN") : o.Token;
                if (string.IsNullOrEmpty(token))
                {
                    Error("The token parameter was not specified and the environment variable TG_TOKEN does not exist or is empty.");
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                var u = string.IsNullOrEmpty(o.ServerUrl) ? Environment.GetEnvironmentVariable("TG_SERVER_URL") : o.ServerUrl;
                if (string.IsNullOrEmpty(u))
                {
                    Error("The server URL parameter was not specified and the environment variable TG_SERVER_URL does not exist or is empty.");
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                if (!Uri.TryCreate(u, UriKind.Absolute, out Uri url))
                {
                    Error("The server URL value {0} is not a valid absolute URI.", u);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                Token = token;
                ServerUrl = url;
                ApiClient = new ApiClient(Token, ServerUrl);
                Debug("Token: {0}. Url: {1}", Token, ServerUrl);
            })
            .WithParsed<PingOptions>(o =>
            {
                Exit(Echo(o).Result);
            });
        }
        #endregion

        #region Methods
        static async Task<ExitResult> Echo(PingOptions o)
        {

            var r = await ApiClient.Echo();
            Info("Echo response: {0}", JsonConvert.SerializeObject(r).ToString());
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
        static Type[] OptionTypes = { typeof(Options), typeof(RestApiOptions), typeof(PingOptions) };

        static string Token { get; set; }

        static Uri ServerUrl { get; set; }

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
