using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TigerGraph.Models;

namespace TigerGraph
{
    interface HttpApiClient
    {
        Task<EchoResponse> Echo(string token);
    }
}
