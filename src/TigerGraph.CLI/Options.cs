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

    public class RestApiOptions : Options
    {
        [Option('t', "token", Required = false, HelpText = "Your TigerGraph server instance access token. If none is specified then use the environment variable TG_TOKEN.")]
        public string Token { get; set; }

        [Option('s', "server", Required = false, HelpText = "Your TigerGraph server instance URL. If none is specified then use the environment variable TG_SERVER_URL.")]
        public string ServerUrl { get; set; }
    }

    [Verb("ping", HelpText = "Ping a TigerGraph server instance using the specified server URL and access token.")]
    public class PingOptions : RestApiOptions {}

}
