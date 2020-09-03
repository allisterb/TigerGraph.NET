using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{

    public class VerticesResult
    {
        public ServerVersion version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public Vertex[] results { get; set; }
    }

    public class ServerVersion
    {
        public string edition { get; set; }
        public string api { get; set; }
        public int schema { get; set; }
    }

    public class Vertex
    {
        public string v_id { get; set; }
        public string v_type { get; set; }
        public Dictionary<string, object> attributes { get; set; }
    }
}
