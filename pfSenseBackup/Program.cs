using pfSenseBackup.Classes;
using System;

namespace pfSenseBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine($"Please run this application with three arguments: the URL of your pfSense machine, your username and password E.g. https://mypfsense:8443 admin password");
                Environment.Exit(1);
            }

            Run(args[0], args[1], args[2]);
        }

        static void Run(string url, string username, string password)
        {
            PfSense pfSense = new PfSense(url, username, password);
            pfSense.DownloadBackup();

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
