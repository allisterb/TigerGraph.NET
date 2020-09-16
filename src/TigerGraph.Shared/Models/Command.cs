using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{
    public class CommandResult
    {
        public string sessionId { get; set; }
        public string serverId { get; set; }
        public string graph { get; set; }
        public int terminalWidth { get; set; }
        public int compileThread { get; set; }
        public bool fromGraphStudio { get; set; }
        public bool fromGsqlClient { get; set; }
        public Dictionary<string, object> sessionParameters { get; set; }
        public bool sessionAborted { get; set; }
        public bool loadingProgressAborted { get; set; }
    }

}
