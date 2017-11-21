using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    public struct HttpHeader
    {
        public string name;
        public string value;
    }

    public struct HttpCookie
    {
        public string name;
        public string value;
    }

    public struct QueryParameters
    {
        public string name;
        public string value;
    }
}
