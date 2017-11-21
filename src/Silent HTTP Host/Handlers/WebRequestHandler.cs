using System;
using System.Collections.Generic;
using System.Net;

namespace Silent_HTTP_Host.Handlers
{
    class WebRequestHandler
    {
        /// <summary>
        /// The raw request data
        /// </summary>
        private string rawRequest;



        /// <summary>
        /// The request mode (GET, POST, PUT, DELETE...)
        /// </summary>
        public string mode;

        /// <summary>
        /// The connecting path
        /// </summary>
        public string path;

        /// <summary>
        /// The GET query from the path
        /// </summary>
        public string query;

        /// <summary>
        /// The protocol version (HTTP/1.1 or HTTP/1.0)
        /// </summary>
        public string protocolVersion;

        /// <summary>
        /// The hostname the user is using to connect
        /// </summary>
        public string connectingHost;

        /// <summary>
        /// The user agent the connecting user is using
        /// </summary>
        public string userAgent;

        /// <summary>
        /// Variable that stores all cookies.
        /// </summary>
        public List<HttpCookie> cookies;

        public List<QueryParameters> queryVariables;



        /// <summary>
        /// The connecting IP address
        /// </summary>
        public string remoteHost;

        /// <summary>
        /// The port used when connecting
        /// </summary>
        public int port;

        /// <summary>
        /// Parsed headers
        /// </summary>
        public List<HttpHeader> headers;

        public static WebRequestHandler ParseRequest(string rawRequest, IPAddress remoteHost, int port)
        {
            WebRequestHandler webRequest = new WebRequestHandler()
            {
                rawRequest = rawRequest,
                port = port,
                remoteHost = remoteHost.ToString(),
                headers = new List<HttpHeader>(),
                cookies = new List<HttpCookie>(),
                queryVariables = new List<QueryParameters>(),
                connectingHost = null,
                userAgent = null,
                mode = null,
                path = null,
                protocolVersion = null,
                query = null,
            };


            #region Headers
            // Parsing HTTP headers...

            int position = 0;
            int lastPosition = 0;
            while (true)
            {
                while (rawRequest[lastPosition] == '\r' || rawRequest[lastPosition] == '\n')
                    lastPosition++;
                while (rawRequest[position] == '\r' || rawRequest[position] == '\n')
                    position++;

                // Checking if the headers have ended
                if (position - 3 > 0 && rawRequest[position - 3] == '\n')
                    break;

                position = rawRequest.IndexOf('\n', position);

                if (position == lastPosition || position == -1)
                    break;

                string headerValue = rawRequest.Substring(lastPosition, position - lastPosition - 1);

                if (headerValue.Length == 0)
                    break;

                int nameEndPosition = headerValue.IndexOf(':');

                if (nameEndPosition == -1)
                {
                    lastPosition = position;
                    continue;
                }

                string headerName = headerValue.Substring(0, nameEndPosition);
                string headerData = headerValue.Substring(nameEndPosition + 1);

                while (headerData[0] == ' ')
                    headerData = headerData.Substring(1);

                if (headerData.Length == 0 || headerName.Length == 0)
                    continue;

                webRequest.headers.Add(new HttpHeader()
                {
                    name = headerName,
                    value = headerData,
                });

                lastPosition = position;
            }

            if (webRequest.headers.Count == 0)
                throw new HttpInvalidHeaderException();
            #endregion

            #region Special Header Extraction
            webRequest.headers.ForEach(delegate(HttpHeader header)
            {
                string name = header.name.ToLower();

                switch (name)
                {
                    case "host":
                        {
                            webRequest.connectingHost = header.value;
                            break;
                        }

                    case "user-agent":
                        {
                            webRequest.userAgent = header.value;
                            break;
                        }

                    case "set-cookie":
                        {
                            // storing this in a temp variable, since the "header"
                            // variable is readonly.
                            string cookieRaw = header.value;

                            // removing the first un-used character.
                            if (cookieRaw[0] == ' ')
                                cookieRaw = cookieRaw.Substring(1);

                            // getting the end position of the name
                            int nameEnding = cookieRaw.IndexOf('=');

                            // Making sure that the = (equal) keyword is there
                            if (nameEnding > -1)
                            {
                                // Storing the cookies in variables
                                string cookieName = cookieRaw.Substring(0, nameEnding);
                                string cookieValue = cookieRaw.Substring(nameEnding + 1);

                                // URL decode the cookies
                                cookieName = Uri.UnescapeDataString(cookieName);
                                cookieValue = Uri.UnescapeDataString(cookieValue);

                                // And adding the cookies...
                                webRequest.cookies.Add(new HttpCookie()
                                {
                                    name = cookieName,
                                    value = cookieValue,
                                });
                            }

                            break;
                        }
                }
            });
            #endregion

            #region Specific Requests

            int modeEndingPosition = rawRequest.IndexOf(' ');
            if (modeEndingPosition == -1)
                throw new HttpInvalidHeaderException();

            int pathEndingPosition = rawRequest.IndexOf(' ', modeEndingPosition + 1);
            if (pathEndingPosition == -1)
                throw new HttpInvalidHeaderException();

            int protocolEndingPosition = pathEndingPosition;
            while (rawRequest[protocolEndingPosition] != '\r' && rawRequest[protocolEndingPosition] != '\r')
                protocolEndingPosition++;

            string mode = rawRequest.Substring(0, modeEndingPosition);
            string path = rawRequest.Substring(modeEndingPosition + 1, pathEndingPosition - modeEndingPosition - 1);
            string protocol = rawRequest.Substring(++pathEndingPosition, protocolEndingPosition - pathEndingPosition);
            #endregion

            #region Query
            int queryPosition = path.IndexOf('?');
            string query = null;
            if (queryPosition > -1)
            {
                query = path.Substring(queryPosition + 1);
                path = path.Substring(0, queryPosition);
            }

            if (query != null && query.Length > 0)
            {
                string[] queryVars = query.Split('&');
                foreach (string s in queryVars)
                {
                    int nameEnding = s.IndexOf('=');
                    if(nameEnding > -1)
                    {
                        webRequest.queryVariables.Add(new QueryParameters()
                        {
                            name = Uri.UnescapeDataString(s.Substring(0, nameEnding)),
                            value = Uri.UnescapeDataString(s.Substring(nameEnding + 1)),
                        });
                    }
                }
            }

            #endregion

            #region Cloudflare IP
            if (Listener.cloudflareMode)
            {
                webRequest.headers.ForEach(delegate(HttpHeader h)
                {
                    if (h.name == "CF-Connecting-IP")
                    {
                        webRequest.remoteHost = h.value;
                    }
                });
            }
            #endregion


            webRequest.mode = mode;
            webRequest.path = Uri.UnescapeDataString(path);
            webRequest.protocolVersion = protocol;
            webRequest.query = query;


            // Logging the request
            Logs.LogConnection(
                webRequest.remoteHost,
                webRequest.path,
                webRequest.query,
                webRequest.connectingHost,
                webRequest.userAgent);
            Console.WriteLine("Connection from {0}:{1} for \"{2}\"", webRequest.remoteHost, port, webRequest.path);


            return webRequest;
        }
    }
}
