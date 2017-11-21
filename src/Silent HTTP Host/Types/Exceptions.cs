using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    class HttpInvalidHeaderException : Exception
    {
        public HttpInvalidHeaderException()
        {

        }
    }

    class HttpUnsupportedModeException : Exception
    {
        public HttpUnsupportedModeException()
        {

        }
    }
}
