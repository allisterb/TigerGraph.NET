using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;

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

    class Program : Runtime
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
            ParserResult<object> result = new Parser().ParseArguments<Options, RestApiOptions, EchoOptions>(args);
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
            .WithParsed<EchoOptions>(o =>
            {
                //CUI(o).Wait();
                Exit(ExitResult.SUCCESS);
            });
        }
        #endregion

        #region Methods
        static void PrintLogo()
        {
            CO.WriteLine(FiggleFonts.Chunky.Render("TigerGraph"), Color.Blue);
            CO.WriteLine("v{0}", AssemblyVersion.ToString(3), Color.Blue);
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
        static Type[] OptionTypes = { typeof(Options), typeof(RestApiOptions), typeof(EchoOptions) };
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
