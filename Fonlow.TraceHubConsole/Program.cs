using System;
using Fonlow.Diagnostics;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fonlow.Logging
{
    class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LoggingConnection init = null;
            try
            {
                init = new LoggingConnection();

                var ok = init.Execute();
                if (!ok)
                    return 1;

                Console.WriteLine("Press Q and Enter to quit the program.");
                Console.WriteLine("Listening to incoming trace messages...");
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input.Equals("Q", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return 0;
                    }
                }

            }
            catch (AbortException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    Console.WriteLine(ex.Message);
                }

                return 2;
            }
            finally
            {
                if (init != null)
                {
                    init.Dispose();
                }
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("There are critical errors so the program will quit. Please resolve the problem before executing this program again. {0}", e.ExceptionObject);
            Trace.TraceError(errorMessage);
            Environment.Exit(666);
        }

    }

}
