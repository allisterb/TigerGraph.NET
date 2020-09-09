using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
#if NET461 || NETSTANDARD || BRIDGE
using Newtonsoft.Json;
#endif
namespace TigerGraph.Models
{
    public class QueryResult
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        
        #if NET461 || NETSTANDARD
        [JsonConverter(typeof(TupleConverter))]
        public Dictionary<string, object> results { get; set; }
        #endif
    }
}