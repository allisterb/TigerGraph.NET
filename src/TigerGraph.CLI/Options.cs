using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;
using CommandLine.Text;

namespace TigerGraph.CLI
{
    public class Options
    {
        [Option('d', "debug", Required = false, HelpText = "Enable debug mode.")]
        public bool Debug { get; set; }
    }

    public class RestApiOptions
    {
        [Option('t', "token", Required = true, HelpText = "Your TigerGraph access token.")]
        public bool Token { get; set; }
    }

    [Verb("echo", HelpText = "Ping the server.")]
    public class EchoOptions : RestApiOptions
    {

    }


}
