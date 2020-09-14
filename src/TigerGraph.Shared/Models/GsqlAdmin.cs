using System;
using System.Collections.Generic;
using System.Text;

namespace TigerGraph.Models
{   
    public class Login
    {
        public bool error { get; set; }
        public LoginResult results { get; set; }
    }

    public class LoginResult
    {
        public bool isGlobalDesigner { get; set; }
        public Roles roles { get; set; }
        public bool isSuperUser { get; set; }
        public string username { get; set; }
    }

    public class Roles : Dictionary<string, string> {}

}
