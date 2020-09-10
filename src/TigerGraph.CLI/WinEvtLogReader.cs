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
            EventData.AddVertexTypes("UserLogon", "Process", "Image");
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
                                var userName = ((string) record.Properties[12].Value);
                                var logonGuid = ((Guid)record.Properties[13].Value).ToString();
                                var logonId = ((ulong)record.Properties[14].Value);
                                if (!EventData.vertices["UserLogon"].ContainsKey(logonGuid))
                                {
                                    EventData.AddVertex("UserLogon", logonGuid).AddVertexAttributes("UserLogon", logonGuid, UpsertOp.ignore_if_exists, ("Id", logonId), ("Name", userName));
                                }
                                var processGuid = ((Guid)record.Properties[2].Value).ToString();
                                var processId = (uint) record.Properties[3].Value;
                                var processImage = ((string)record.Properties[4].Value).ToString();
                                var commandLine = ((string)record.Properties[10].Value).ToString();
                                var currentDirectory = ((string)record.Properties[11].Value).ToString();
                                var terminalSessionId = ((uint)record.Properties[15].Value).ToString();
                                if (!EventData.vertices["Process"].ContainsKey(processGuid))
                                {
                                    EventData.AddVertex("Process", processGuid).AddVertexAttributes("Process", processGuid, 
                                        UpsertOp.ignore_if_exists, ("Id", processId), ("Image", processImage), ("CommandLine", commandLine), ("CurrentDirectory", currentDirectory));
                                }
                                break;
                            case 3:
                                //Debug("Reading {0} event properties {1}.", "Connection", record.Properties.Select((p, i) => i.ToString() + ":" + p.Value));
                                break;
                            default:
                                continue;
                        }
                        bookmark = record.Bookmark;
                    }
                    Info("Read {0} process create events.", EventData.vertices["Process"].Count);
                }
            }
        }
        protected EventBookmark bookmark;
    }
}
#endif