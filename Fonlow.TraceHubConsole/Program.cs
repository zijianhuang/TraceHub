using System;
using Fonlow.Diagnostics;

namespace Fonlow.Logging
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var init = new LoggingConnection();

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    init.Dispose();//but this won't be called if the process is terminated disgracefully.
                };

                var ok = init.Execute();
                if (!ok)
                    return;

                Console.WriteLine("Press Q and Enter to quit the program.");
                Console.WriteLine("Listening to incoming trace messages...");
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
                    {
                       // init.Dispose();
                        Environment.Exit(0);
                    }
                }

            }
            catch (AbortException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    Console.WriteLine(ex.Message);
                }

                Environment.Exit(0);
            }
        }
    }

}
