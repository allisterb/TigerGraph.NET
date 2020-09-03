using System;
using System.Collections.Generic;
using System.Text;

#if WS
using WebSharper;
#endif

namespace TigerGraph.Models
{
#if WS
    [JavaScript]
#endif
    public class EchoResponse
    {
        public Version version { get; set; }
        public bool error { get; set; }
        public string message { get; set; }
        public string code { get; set; }
    }

#if WS
    [JavaScript]
#endif
    public class Version
    {
        public string edition { get; set; }
        public string api { get; set; }
        public int schema { get; set; }
    }
}
