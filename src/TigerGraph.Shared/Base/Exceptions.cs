using System;
using System.Collections.Generic;
using System.Text;

#if WS
using WebSharper;
#endif

namespace TigerGraph
{
#if WS
    [JavaScript]
#endif
    public class RuntimeNotInitializedException : Exception
    {
        public RuntimeNotInitializedException(Base.Runtime o) : base($"This runtime object is not initialized.") {}
    }

}
