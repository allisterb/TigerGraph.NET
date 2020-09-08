using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class QueryResult
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> results { get; set; }
    }

}
