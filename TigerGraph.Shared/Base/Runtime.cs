using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TigerGraph
{
    public abstract class Runtime
    {
        #region Constructors
        static Runtime()
        {
            //HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SMApp/0.1");
        }
        public Runtime(CancellationToken ct)
        {
            if (Logger == null)
            {
                throw new InvalidOperationException("A logger is not assigned.");
            }
            CancellationToken = ct;
            Type = this.GetType();
        }
        public Runtime(): this(Cts.Token) {}

        #endregion

        #region Properties
        //public static DirectoryInfo AssemblyDirectory { get; } = new FileInfo(Assembly.GetEntryAssembly().Location).Directory;

        //public static Version AssemblyVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;

        //public static DirectoryInfo CurrentDirectory { get; } = new DirectoryInfo(Directory.GetCurrentDirectory());
     
        public static Logger Logger { get; protected set; }

        public static CancellationTokenSource Cts { get; } = new CancellationTokenSource();

        public static CancellationToken Ct { get; } = Cts.Token;

        

        public static string YY = DateTime.Now.Year.ToString().Substring(2, 2);

        public bool Initialized { get; protected set; }

        public CancellationToken CancellationToken { get; protected set; }

        public Type Type { get; }

        #endregion

        #region Methods
        public static void SetLogger(Logger logger)
        {
            Logger = logger;
        }

        public static void SetLoggerIfNone(Logger logger)
        {
            if (Logger == null)
            {
                Logger = logger;
            }
        }

        public static void SetDefaultLoggerIfNone()
        {
            if (Logger == null)
            {
                Logger = new ConsoleLogger();
            }
        }

        public static void Info(string messageTemplate, params object[] args) => Logger.Info(messageTemplate, args);

        public static void Debug(string messageTemplate, params object[] args) => Logger.Debug(messageTemplate, args);

        public static void Error(string messageTemplate, params object[] args) => Logger.Error(messageTemplate, args);

        public static void Error(Exception ex, string messageTemplate, params object[] args) => Logger.Error(ex, messageTemplate, args);

        public static Logger.Op Begin(string messageTemplate, params object[] args) => Logger.Begin(messageTemplate, args);

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

        public void ThrowIfNotInitialized()
        {
            if (!this.Initialized) throw new ApiNotInitializedException(this);
        }
        #endregion
    }
}
