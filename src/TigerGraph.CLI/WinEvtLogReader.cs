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

using TigerGraph.Models;

namespace TigerGraph.CLI
{
    public class WinEvtLogReader : Base.Runtime
    {
        public WinEvtLogReader(Upsert eventData)
        {
            SysMonLogQuery = new EventLogQuery(Program.SysMonEvtLogPath, PathType.FilePath);
            EventData = eventData;
            EventData.AddVertexTypes("User", "Process", "Image", "Logon");
            Initialized = true;
        }

        public EventLogQuery SysMonLogQuery { get; }
        
        public Upsert EventData { get; }
        public void ReadSysMonLog()
        {
            while (!Console.KeyAvailable && !Ct.IsCancellationRequested)
            {
                EventRecord record;
                using (var reader = bookmark != null ? new EventLogReader(SysMonLogQuery, bookmark) : new EventLogReader(SysMonLogQuery))
                {
                    while (!Console.KeyAvailable && !Ct.IsCancellationRequested && (record = reader.ReadEvent()) != null)
                    {
                        switch(record.Id)
                        {
                            case 1:
                                Debug("Reading {0} event properties {1}.", "Process Create", record.Properties.Select((p, i) => i.ToString() + ":" + p.Value));
                                var processId = (string) record.Properties[2].Value;
                                var attrs = new Dictionary<string, object>();
                                break;
                            case 3:
                                Debug("Reading {0} event properties {1}.", "Connection", record.Properties.Select((p, i) => i.ToString() + ":" + p.Value));
                                break;
                            default:
                                continue;
                        }
                        bookmark = record.Bookmark;
                    }
                }
            }
        }
        protected EventBookmark bookmark;
    }
}
#endif