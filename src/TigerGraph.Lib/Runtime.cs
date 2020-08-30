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
        }
        public Runtime(CancellationToken ct) :base(ct) { Type = GetType(); }
        public Runtime() : this(Cts.Token) { }

        #endregion

        #region Properties
        public static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;

        public static Version AssemblyVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;

        public static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static IConfigurationRoot Configuration { get; protected set; }
   
        public Type Type { get; }
        #endregion

        #region Methods
        public static string Config(string i) => Runtime.Configuration[i];

        public static void SetPropsFromDict<T>(T instance, Dictionary<string, object> p)
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(instance, p[prop.Name]);
                }
            }
        }

        #endregion
    }
}
