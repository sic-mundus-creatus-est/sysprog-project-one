using System;
using System.Diagnostics;
using CapitalWordCounter;
class Program
{
    static void Main(string[] args)
    {
        try
        {
            string rootFolder = @".\root"; // <== file searching always starts here

            //====================================================
            // To check if a port is avaliable on your PC:      //
            //          Windows: netstat -ano | findstr :18859  //
            //          Linux: netstat -tuln | grep 18859       //
            //====================================================
            string serverURL = "http://localhost:18859/"; // <== also a "homepage"

            CWC_Server server = new CWC_Server(rootFolder, serverURL);
            server.Start();



            Console.ReadKey(); // <== stops the server from shutting down until you press any key

            server.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine("  x Unexpected error: " + ex.Message);
        }
    }
}
