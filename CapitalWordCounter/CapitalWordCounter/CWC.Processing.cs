using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CapitalWordCounter
{
    partial class CWC_Server
    {  
    //====================================================================================================================================
    // *** CWC SERVER PROCESSING FUNCTIONS *** //
    //====================================================================================================================================
    //
    //
        //=============================================================================================================
        /// <summary>
        /// Processes incoming HTTP GET requests, generating appropriate responses based on the request URL path.
        /// </summary>
        /// <param name="state">The HTTP context representing the incoming request.</param>
        //=============================================================================================================
        private void ProcessRequest(object state)
        {
            HttpListenerContext context = (HttpListenerContext)state;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.HttpMethod != "GET")
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close();
                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Received non-GET request: {request.HttpMethod} {request.Url}");
                return;
            }

            Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : New request received: GET {request.Url}");

            if (request.Url.AbsolutePath == "/favicon.ico")
            {// to get rid of the annoying favicon request...
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Close();
                Console.WriteLine("  # An request was invalid: GET /favicon.ico - 403 Forbidden");
                return;
            }

            if (request.Url.AbsolutePath == "/")
            {// returns the "homepage" as a response
                byte[] buffer = Encoding.UTF8.GetBytes(_homepageFrontend);
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);

                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Response sent successfully for request: GET {request.Url}");
            }
            else
            {
                string urlQuery = request.Url.AbsolutePath;

                if (IsSinglePath(urlQuery))
                {
                    string fileName = urlQuery.TrimStart('/');

                        try
                        {
                            string responseBody = "";
                            string wordCount = "";

                            wordCount += _cache.GetOrCreate(fileName, RootFolder, CountCapitalWords);

                            Console.WriteLine(responseBody);

                            responseBody = GenerateResponse(fileName, wordCount);

                            byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                            response.ContentType = "text/html";
                            response.ContentLength64 = buffer.Length;
                            response.StatusCode = 200;
                            response.OutputStream.Write(buffer, 0, buffer.Length);

                            Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Response sent successfully for request: GET {request.Url}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  # Error processing request: {ex.Message}");
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }

                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Console.WriteLine($"  # Bad request received: GET {request.Url} - 400 Bad Request");

                }
            }
            response.Close();
        }


        //=======================================================================================
        /// <summary>
        /// Determines whether the URL path consists of a single segment.
        /// </summary>
        /// <param name="url">The URL path to evaluate.</param>
        /// <returns>True if the URL path contains only one segment; otherwise, false.</returns>
        //=======================================================================================
        private bool IsSinglePath(string url)
        {
            int slashCount = 0;

            foreach (char c in url)
            {
                if (c == '/')
                {
                    slashCount++;

                    if (slashCount > 1)
                    {
                        return false;
                    }
                }
            }

            return slashCount == 1;
        }


        //====================================================================================================
        /// <summary>
        /// Recursively searches for a specified file within the given root directory and its subdirectories.
        /// </summary>
        /// <param name="root">The root directory to start the search from.</param>
        /// <param name="fileName">The name of the file to search for.</param>
        /// <returns>The full path of the first occurrence of the file found, or null if not found.</returns>
        //====================================================================================================
        public static string SearchFile(string root, string fileName)
        {
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException($"Root directory '{root}' not found.");
            }

            string[] files = Directory.GetFiles(root, fileName);

            if (files.Length > 0)
            {// if the file is found in the root directory, returns its path
                return files[0];
            }

            // otherwise, does recursive search through all subdirectories of root
            string[] subDirectories = Directory.GetDirectories(root);
            foreach (string subDirectory in subDirectories)
            {
                try
                {
                    string filePath = SearchFile(subDirectory, fileName);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        return filePath;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // ignores unauthorized access exceptions and continues searching
                }
            }

            return null;
        }


        //============================================================================================
        /// <summary>
        /// Counts the words in a file that begin with a capital letter and have more than 5 letters.
        /// </summary>
        /// <param name="filePath">The path of the file to analyze.</param>
        /// <returns>A string representing the total count of such words in the file.</returns>
        //============================================================================================
        public string CountCapitalWords(string filePath)
        {
            int totalCount = 0;
            object lockObject = new object();
            CountdownEvent countdownEvent = new CountdownEvent(1);

            using (StreamReader reader = new StreamReader(filePath))
            {
                StringBuilder paragraphBuilder = new StringBuilder();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    while (!string.IsNullOrWhiteSpace(line))
                    {
                        paragraphBuilder.AppendLine(line);
                        line = reader.ReadLine();
                    }

                    string paragraph = paragraphBuilder.ToString().Trim();
                    if (!string.IsNullOrEmpty(paragraph))
                    {
                        countdownEvent.AddCount();

                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            string paragraphToProcess = (string)state;

                            int localCount = 0;

                            string[] words = paragraphToProcess.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string word in words)
                            {
                                if (word.Length > 5 && char.IsUpper(word[0]))
                                {
                                    localCount++;
                                }
                            }

                            lock (lockObject)
                            {
                                totalCount += localCount;
                            }

                            countdownEvent.Signal();

                        }, paragraph);

                        paragraphBuilder.Clear();
                    }
                }
            }
            countdownEvent.Signal(); // signals the main thread of the request that all paragraphs are queued
            countdownEvent.Wait(); // waits for all worker threads to finish

            return totalCount.ToString();
        }

    }
}
