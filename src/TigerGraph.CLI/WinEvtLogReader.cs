using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            EventLogs = EventLog.GetEventLogs();
              
        }

        public static EventLog[] EventLogs { get; private set; }
    }
}
