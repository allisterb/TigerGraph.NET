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

    public class ApiOptions : Options
    {
        [Option('t', "token", Required = false, HelpText = "Your TigerGraph server instance access token. If none is specified then use the environment variable TG_TOKEN.")]
        public string Token { get; set; }

        [Option('r', "rest", Required = false, HelpText = "Your TigerGraph REST++ server instance URL. If none is specified then use the environment variable TG_REST_SERVER_URL.")]
        public string RestServerUrl { get; set; }

        [Option('q', "gsql", Required = false, HelpText = "Your TigerGraph REST++ server instance URL. If none is specified then use the environment variable TG_GSQL_SERVER_URL.")]
        public string GsqlServerUrl { get; set; }

        [Option('u', "user", Required = false, HelpText = "Your TigerGraph server user name. If none is specified then use the environment variable TG_USR.")]
        public string User { get; set; }

        [Option('p', "pass", Required = false, HelpText = "Your TigerGraph server user password. If none is specified then use the environment variable TG_PASS.")]
        public string Pass { get; set; }
    }

    [Verb("ping", HelpText = "Ping a TigerGraph server instance using the specified server URL and access token.")]
    public class PingOptions : ApiOptions {}

}
