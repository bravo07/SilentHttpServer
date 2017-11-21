using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    static class Misc
    {
        public const uint time_second = 60;
        public const uint time_minute = time_second * 60;
        public const uint time_hour = time_minute * 60;
        public const uint time_day = time_hour * 24;
        public const uint time_week = time_day * 7;

        public static uint time
        {
            get
            {
                return (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
        }

        public static string GetFormattedDate(uint time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(time).ToLocalTime();
            return dtDateTime.ToString("r");
        }

        /// <summary>
        /// Parses a file location.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ParseFileLocation(string fileName)
        {
            fileName = fileName.Replace("{CURRENT_DIR}", Environment.CurrentDirectory);
            fileName = fileName.Replace("{APPDATA}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            fileName = fileName.Replace('/', '\\');

            return fileName;
        }

        /// <summary>
        /// Escapes HTML characters.
        /// </summary>
        public static string HtmlEscape(string input)
        {
            // Probably should add support for more encodings, or
            // just black list all other encodings. C:::
            return input
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        public static string BoolToString(this bool boolean)
        {
            if (boolean)
                return "1";
            else
                return "0";
        }

        public static bool StringToBool(this string boolean)
        {
            if (boolean == "0")
                return false;
            if (boolean == "1")
                return true;
            throw new InvalidCastException();
        }
    }
}
