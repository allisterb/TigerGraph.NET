using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class BuiltinRequest
    {
        public string function { get; set; }
        public string type { get; set; }
    }

    public class BuiltinResponse
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public BuiltinResult[] results { get; set; }
    }

    public class BuiltinResult
    {
        public string v_type { get; set; }

        public string e_type { get; set; }

        public Dictionary<string, Dictionary<string, object>> attributes { get; set; }
    }
}
