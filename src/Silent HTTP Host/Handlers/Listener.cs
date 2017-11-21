using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Handlers
{
    class Listener
    {
        #region Connection Status

        /// <summary>
        /// This class is supposed to keep track of the current
        /// connection status.
        /// </summary>
        private static class ConnectionStatus
        {
            enum ConnectionState : byte
            {
                ALLOW_LISTENING,
                DISALLOW_LISTENING
            };

            private static ConnectionState connectionState =
                ConnectionState.ALLOW_LISTENING;

            public static void WaitForAllowedListening()
            {
                while (connectionState == ConnectionState.DISALLOW_LISTENING)
                {
                    Task.Delay(5).Wait();
                }
            }

            public static void AllowListening()
            {
                connectionState = ConnectionState.ALLOW_LISTENING;
            }

            public static void DisallowListening()
            {
                connectionState = ConnectionState.DISALLOW_LISTENING;
            }
        }
        #endregion

        /// <summary>
        /// Headers that will be injected
        /// </summary>
        private static readonly HttpHeader[] defaultHeaders =
        {
            new HttpHeader()
            {
                name = "Server",
                value = "Silent HTTP Server"
            },
        };

        /// <summary>
        /// The maximum size a request can be.
        /// </summary>
        public static uint maximumRequestSize = 1024 * 1024 * 10;

        /// <summary>
        /// The buffer size for reading data from client
        /// </summary>
        public static int bufferSize = 1024;

        /// <summary>
        /// This will get the client ip from CF-Connecting-IP
        /// header, rather then the connecting tcp client
        /// </summary>
        public static bool cloudflareMode = false;

        /// <summary>
        /// The listening port
        /// </summary>
        public static int port = 80;


        public static void StartListener()
        {
            // Getting the listening endpoint
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine("Listening on port {0}", port);


            // Initializing listener
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);


            // Binding listening location with listener
            listener.Bind(localEndPoint);
            Console.WriteLine("Binded end point");


            // Starting the listener
            listener.Listen(int.MaxValue);
            Console.WriteLine("Started listener");
            Console.Write("==============\n\n");

            while (true)
            {
                ConnectionStatus.DisallowListening();


                // Begin accepting requests
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);


                // Incrementing request count
                Analytics.IncrementRequests();


                // Updating the title to the amount of requests that
                // have happened in this current session.
                Console.Title = string.Format("Request Count: {0}",
                    Analytics.GetRequestCount().ToString());


                // Waiting for connnection to be made
                ConnectionStatus.WaitForAllowedListening();
            }
        }

        /// <summary>
        /// Accepts the callback from client, also handles parsing the
        /// request, and sending response.
        /// </summary>
        public static void AcceptCallback(IAsyncResult ar)
        {
            // Connection has been made, let's let the main thread continue
            ConnectionStatus.AllowListening();

            int t = Environment.TickCount;

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);


            // Buffer for receiving data
            byte[] buffer = new byte[bufferSize];


            // Raw request
            StringBuilder rawRequest = new StringBuilder();


            // Fetching data from server
            int receivedCount = 0;
            int requestSize = 0;
            while (true)
            {
                receivedCount = handler.Receive(
                    buffer,
                    0,
                    bufferSize,
                    SocketFlags.None);

                requestSize += receivedCount;
                if (requestSize > maximumRequestSize)
                {
                    // Exceeded maxiumum request size

                    // TODO: Send a page to client saying that the
                    // request buffer is too large.

                    // For now, just clearing some variables
                    rawRequest.Clear();
                    buffer = null;

                    // Just logging the failed request (IK!! IK!! It's very fucking long)
                    Console.WriteLine("New request from {0}, but the request was too large.", (cloudflareMode ? string.Format("(CFIP){0}", (handler.RemoteEndPoint as IPEndPoint).ToString()) : (handler.RemoteEndPoint as IPEndPoint).ToString()));

                    return;
                }
                else
                {
                    rawRequest.Append(Encoding.ASCII.GetString(buffer));

                    // Checking if this is the end of the receive
                    // buffer.
                    if (receivedCount < bufferSize)
                        break;
                }
            }

            // Getting end point
            IPEndPoint ipep = (handler.RemoteEndPoint as IPEndPoint);


            // Storing request/response in variables for ease access
            WebRequestHandler request = default(WebRequestHandler);
            WebResponseHandler response = default(WebResponseHandler);


            try
            {
                // Generating a request
                request = WebRequestHandler.ParseRequest(
                    rawRequest.ToString(), ipep.Address, ipep.Port);


                // Generating a response
                response = new WebResponseHandler(request.path, ref request);


                // Getting a string builder for the response
                StringBuilder responseData = new StringBuilder();


                // Appending main header (not sure if it has a name or what)
                responseData.Append(string.Format("{0} {1} {2}\r\n",
                    request.protocolVersion, response.StatusCode.ToString(),
                    response.StatusCodeDescriptor));


                // CGI status (Non-standard)
                responseData.Append(string.Format("Status: {0} {1}", response.StatusCode, response.StatusCodeDescriptor));


                // Appending default headers
                foreach (HttpHeader header in defaultHeaders)
                    responseData.Append(string.Format("{0}: {1}\r\n",
                        header.name, header.value));


                // Appending essential headers
                responseData.Append(string.Format("Date: {0}\r\n",
                    Misc.GetFormattedDate(Misc.time)));


                // Adding all headers
                foreach (HttpHeader header in response.Headers)
                {
                    string value = header.value;

                    // If the value contains a new line, you could
                    // potentially inject extra headers, which is not good.
                    if (value.Contains('\r') || value.Contains('\n')
                        || header.name.Contains('\r') || header.name.Contains('\n'))
                        return;

                    // Writing header to buffer
                    responseData.Append(string.Format("{0}: {1}\r\n", header.name, value));
                }


                // If the content isn't empty, add the required headers
                if (!string.IsNullOrEmpty(response.Content))
                {
                    // TODO: Check if content-length is already set.
                    responseData.Append(string.Format("Content-Length: {0}\r\n",
                        response.Content.Length.ToString()));

                    responseData.Append("\r\n");
                    responseData.Append(response.Content);
                }


                // Getting a string to send, we need a reference
                string rawSend = responseData.ToString();


                // Sending the response.
                Send(ref handler, ref rawSend);


                // nullifying variables
                response.Clear();
                rawRequest.Clear();
                responseData.Clear();
                buffer = null;
                rawSend = null;
                request = null;
                response = null;

                Console.WriteLine(Environment.TickCount - t);

                // Collect all this shit, don't want to wait for it
                // to collect them.
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
            catch (Exception ex)
            {
                Logs.LogException(ex.ToString());
            }
        }

        private static void Send(ref Socket handler, ref string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);


            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                
                handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                // If connection on client is closed forcibly, this will
                // throw an exception. I don't want this whole program
                // crashing if that happens.
                Logs.LogException(ex.ToString());
            }
        }
    }
}
