using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CapitalWordCounter
{
    partial class CWC_Server
    {
    //===============================================
    // *** CWC SERVER ATTRIBUTES *** //
    //===============================================
        public string RootFolder { get; set; }
        private string _homepageFrontend;
        private string _serverURL;
        private readonly HttpListener _httpListener;
        private CWC_Cache _cache;
        private int _requestsHandled;
    //===============================================


        //==================================================================
        // *** CWC SERVER CONSTRUCTOR *** //
        //==================================================================
        /// <summary>
        /// CapitalWordCounter Server Constructor.
        /// </summary>
        //====================-----------------------=======================
        public CWC_Server(string rootFolder, params string[] prefixes)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string appName = assembly.GetName().Name;

            Console.WriteLine("=======================================\n");
            Console.WriteLine("=> Booting up the server...");
            Console.WriteLine($"  # Serving C# app: {appName}");
            Console.WriteLine("  # Environment: development\n");
            Console.WriteLine("=> Starting the server...");

            RootFolder = rootFolder;
            _serverURL = prefixes[0];
            _httpListener = new HttpListener();

            foreach (string prefix in prefixes)
            {
                _httpListener.Prefixes.Add(prefix);
            }
            Console.WriteLine($" # Listening on {prefixes[0]}\n");

            _cache = new CWC_Cache(124);
            _homepageFrontend = GenerateHomepage();

            _requestsHandled = 0;
        }


    //====================================================================================================================================
    // *** CWC SERVER MANAGEMENT FUNCTIONS *** //
    //====================================================================================================================================
    //
    //
        //================================
        /// <summary>
        /// Attempts to start the server.
        /// </summary>
        //========--------------==========
        public void Start()
        {
            try
            {
                _httpListener.Start();
                ThreadPool.QueueUserWorkItem(Listen);
                Console.WriteLine("=> Server started...\n");
                Console.WriteLine("=======================================");
                Console.WriteLine("(Press any key to stop)\n");

                OpenUrl();
            }
            catch(Exception e)
            {
                Console.WriteLine($"  x Error occured while starting the server: {e.Message}");
                if (_httpListener != null && _httpListener.IsListening)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
                Console.WriteLine("=> Server stopped forcefully due to an error.");
                Environment.Exit(1);
            }
        }

        //===============================
        /// <summary>
        /// Attempts to stop the server.
        /// </summary>
        //========-------------==========
        public void Stop()
        {
            try
            {
                Console.WriteLine("=> Attempting to stop the server...");
                _httpListener.Stop();
                _httpListener.Close();
                Console.WriteLine("=> Server stopped successfully.");
                Console.WriteLine($"=> Total number of requests handled during the runtime: {_requestsHandled}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  x Error occured while stopping the server: {e.Message}");

                Console.WriteLine("=> Server stopped forcefully due to an error.");
                Environment.Exit(1);
            }
        }

        //========================
        /// <summary>
        /// Listens for requests.
        /// </summary>
        //=======----------=======
        private void Listen(object state)
        {
            while (_httpListener.IsListening)
            {
                ThreadPool.QueueUserWorkItem(ProcessRequest, _httpListener.GetContext());
                _requestsHandled++;
            }
        }

        //==========================================================================
        /// <summary>
        /// Attempts to open the server URL in the default browser on Server Start.
        /// </summary>
        /// <remarks>
        /// Note: Only works for Windows and Linux.
        /// </remarks>
        //===============-------------------------------------------================
        private void OpenUrl()
        {
            try
            {
                Process.Start(_serverURL);
            }
            catch(Exception e)
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _serverURL = _serverURL.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(_serverURL) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", _serverURL);
                }
                else
                {
                    Console.WriteLine("  x Error occurred while attempting to open the homepage URL in the default browser: " + e.Message);
                }
            }
        }

    //====================================================================================================================================
    // *** END *** //
    //====================================================================================================================================
    }
}
