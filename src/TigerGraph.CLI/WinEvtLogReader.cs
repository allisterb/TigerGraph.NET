#if WINDOWS && NET461
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TigerGraph.CLI
{
    public class LogReader : Base.Runtime
    {
        static LogReader()
        {
                          
        }

        public static EventBookmark Bookmark;
        public LogReader()
        {
            var q = new EventLogQuery(Program.SysMonEvtLogPath, PathType.FilePath);
            EventRecord record;
            using (var reader = new EventLogReader(q))
            {
                while ((record = reader.ReadEvent()) != null)
                {
                    //record.
                }
                
            }
                //EventLogQuery eventsQuery = new EventLogQuery(LogName, PathType.LogName, query) { ReverseDirection = true };
                //EventLogReader logReader = new EventLogReader(eventsQuery);
                //Info("Event logs: {0}", EventLogs.Select(l => l.Log));
                //var l = EventLog.
        }
        public static EventLog[] EventLogs { get; private set; }
    }
}
#endif