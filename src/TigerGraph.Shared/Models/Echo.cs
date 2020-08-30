using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class EchoResponse
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public string code { get; set; }
    }

    public class Version
    {
        public string edition { get; set; }
        public string api { get; set; }
        public int schema { get; set; }
    }
}
