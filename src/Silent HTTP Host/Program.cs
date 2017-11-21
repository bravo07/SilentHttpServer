using JSON;
using Silent_HTTP_Host.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    class Program
    {
        static void Main(string[] args)
        {
            // Inline exception handler.
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Logs.LogException(e.ExceptionObject.ToString());
            };

            // Initializing Configuration
            Config.LoadConfig();

            #region First time configuration
            if (!Config.Exists("main", "initialized"))
            {
                Console.WriteLine("Configuration Setup\n");
                // Hasn't configured client



                //
                // Listening port
                //
                if (!Config.Exists("main", "listeningPort"))
                {
                    Console.WriteLine("Listening Port:");
                    int listeningPort = 0;
                    while (!int.TryParse(Console.ReadLine(), out listeningPort))
                        Console.WriteLine("Listening Port:");

                    Config.SetInt("main", "listeningPort", listeningPort);
                }



                //
                // Cloduflare option
                //
                if (!Config.Exists("main", "cloudflareEnabled"))
                {
                    Console.WriteLine("Do you want to enable cloudflare?[Y/n]:");
                    bool cloudflareEnabled = false;
                    while (true)
                    {
                        string enableCloudflare = Console.ReadLine().ToLower();
                        if (enableCloudflare == "y")
                        {
                            cloudflareEnabled = true;
                            break;
                        }
                        else if(enableCloudflare == "n")
                        {
                            cloudflareEnabled = false;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Do you want to enable cloudflare?[Y/n]:");
                            continue;
                        }
                    }

                    Config.SetString("main", "cloudflareEnabled",
                        cloudflareEnabled.BoolToString());
                }


                Config.SetString("main", "initialized", "1");
                Config.Save();

                Console.WriteLine("\nConfiguration setup complete\n==============\n");
            }
            #endregion
            #region Loading Configuration
            if (Config.Exists("main", "listeningPort"))
            {
                Listener.port = Config.ReadInteger("main", "listeningPort");
                Console.WriteLine("Port: {0}", Listener.port);
            }

            if (Config.Exists("main", "cloudflareEnabled"))
            {
                Listener.cloudflareMode = Config.ReadString("main",
                    "cloudflareEnabled").StringToBool();
                Console.WriteLine("Cloudflare Mode Enabled: {0}", Listener.cloudflareMode);
            }
            Console.WriteLine("==============\n");
            #endregion

            // Starting the http listener
            Listener.StartListener();
        }
    }
}
