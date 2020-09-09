using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class EndPointParameter
    {
        public string _default { get; set; }
        public int max_count { get; set; }
        public int min_count { get; set; }
        public string type { get; set; }

        public string options { get; set; }
    }

    public class Endpoint
    {
        public Dictionary<string, EndPointParameter> parameters { get; set; }
    }

}








