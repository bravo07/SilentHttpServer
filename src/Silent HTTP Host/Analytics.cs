using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * TODO: Make the analytics a little more complete, it's a little hard
 * without being able to use a SQL database.
 */

namespace Silent_HTTP_Host
{
    class Analytics
    {
        private static uint requestCount = 0;

        public static void IncrementRequests()
        {
            if (requestCount + 1 <= uint.MaxValue)
                ++requestCount;
        }

        public static uint GetRequestCount()
        {
            return requestCount;
        }
    }
}
