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
    public class WinEvtLogReader : Base.Runtime
    {
        public WinEvtLogReader()
        {
            SysMonLogQuery = new EventLogQuery(Program.SysMonEvtLogPath, PathType.FilePath);
            Initialized = true;
        }

        public EventLogQuery SysMonLogQuery { get; }
        
        
        public void ReadSysMonLog()
        {
            while (!Console.KeyAvailable && !Ct.IsCancellationRequested)
            {
                EventRecord record;
                using (var reader = bookmark != null ? new EventLogReader(SysMonLogQuery, bookmark) : new EventLogReader(SysMonLogQuery))
                {
                    while (!Console.KeyAvailable && !Ct.IsCancellationRequested && (record = reader.ReadEvent()) != null)
                    {
                        Info("Keywords: {0}", record.OpcodeDisplayName);
                    }

                }
            }
        }
        protected EventBookmark bookmark;
    }
}
#endif