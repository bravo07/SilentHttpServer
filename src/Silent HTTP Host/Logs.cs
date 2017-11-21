using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    class Logs
    {
        /// <summary>
        /// Logs a connection to the connection logs file
        /// </summary>
        public static void LogConnection(string remoteIp, string path, string query, string connectingHost, string userAgent)
        {
            string filePath = Misc.ParseFileLocation("{CURRENT_DIR}/connection_logs.txt");

            if (!File.Exists(filePath))
                File.Create(filePath);

            uint time = Misc.time;

            // de-nullifying
            if (remoteIp == null)
                remoteIp = string.Empty;
            if (path == null)
                path = string.Empty;
            if (query == null)
                query = string.Empty;
            if (connectingHost == null)
                connectingHost = string.Empty;
            if (userAgent == null)
                userAgent = string.Empty;


            // Escaping lines
            remoteIp = remoteIp.Replace("\r", "\\r");
            remoteIp = remoteIp.Replace("\n", "\\n");
            path = path.Replace("\n", "\\n");
            path = path.Replace("\n", "\\n");
            query = query.Replace("\n", "\\n");
            query = query.Replace("\n", "\\n");
            connectingHost = connectingHost.Replace("\n", "\\n");
            connectingHost = connectingHost.Replace("\n", "\\n");
            userAgent = userAgent.Replace("\n", "\\n");
            userAgent = userAgent.Replace("\n", "\\n");

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write(string.Format("{0} @ {1}\r\n{2}{3}?{4}\r\n{5}\r\n\r\n", remoteIp, string.Format("{0} ({1})", time, Misc.GetFormattedDate(time)), connectingHost, path, query, userAgent));
                sw.Close();
            }
        }

        /// <summary>
        /// When an error occurs, this will log it to the exception log file
        /// </summary>
        /// <param name="exceptionDetails"></param>
        public static void LogException(string exceptionDetails)
        {
            Console.WriteLine("Exception occured, see logs for more details");

            string filePath = Misc.ParseFileLocation("{CURRENT_DIR}/exceptions.txt");

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
                Task.Delay(1).Wait();
            }

            uint time = Misc.time;

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.Write(string.Format("{0} ({1})\r\n{2}\r\n\r\n\r\n", time.ToString(), Misc.GetFormattedDate(time), exceptionDetails));
                sw.Close();
            }
        }
    }
}
