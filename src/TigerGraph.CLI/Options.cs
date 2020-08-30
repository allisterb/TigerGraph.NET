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
        [Option('t', "token", Required = false, HelpText = "Your TigerGraph server instance access token. If none is specified then use the environment variable TG_TOKEN.")]
        public bool Token { get; set; }

        [Option('s', "server", Required = true, HelpText = "Your TigerGraph server instance URL. If none is specified then use the environment variable TG_SERVER_URL.")]
        public Uri ServerUrl { get; set; }
    }

    [Verb("echo", HelpText = "Ping the specified server using the specified access token.")]
    public class EchoOptions : RestApiOptions {}

}
