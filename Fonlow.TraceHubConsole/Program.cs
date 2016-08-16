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
                        Environment.Exit(0);
                    }
                }

            }
            catch (AbortException)
            {
                Environment.Exit(0);
            }
        }
    }

}
