using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace TigerGraph
{
    public class Runtime : Base.Runtime
    {
        #region Constructors
        static Runtime()
        {
            Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SMApp/0.1");
        }
        public Runtime(CancellationToken ct) :base(ct) {}
        public Runtime() : this(Cts.Token) { }

        #endregion

        #region Properties
        public static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;

        public static Version AssemblyVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;

        public static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static IConfigurationRoot Configuration { get; protected set; }
   
        public static HttpClient HttpClient { get; } = new HttpClient();
        #endregion
    }
}
