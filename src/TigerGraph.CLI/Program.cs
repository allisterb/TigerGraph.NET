using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;

using CO = Colorful.Console;
using Figgle;
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
        static void Main(string[] args)
        {
            SetLogger(new SerilogLogger(console: true, debug: false));
            Console.WriteLine("Hello World!");
        }

        static void PrintLogo()
        {
            CO.WriteLine(FiggleFonts.Chunky.Render("SMApp"), Color.Blue);
            CO.WriteLine("v{0}", AssemblyVersion.ToString(3), Color.Blue);
        }

        #region Properties
        #endregion
        public static void Exit(ExitResult result)
        {

            if (Cts != null && !Cts.Token.CanBeCanceled)
            {
                Cts.Cancel();
                Cts.Dispose();
            }

            Environment.Exit((int)result);
        }
    }
}
