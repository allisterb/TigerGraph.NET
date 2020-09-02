using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TigerGraph.Models;

namespace TigerGraph
{
    interface IApiClient
    {
        Task<T> RestHttpGetAsync<T>(string query);
    }
}
