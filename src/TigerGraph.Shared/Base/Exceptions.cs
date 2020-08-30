using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph
{
    public class RuntimeNotInitializedException : Exception
    {
        public RuntimeNotInitializedException(Base.Runtime o) : base($"The {o.Type.Name} runtime object is not initialized.") {}
    }

}
