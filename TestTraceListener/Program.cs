using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestTraceListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program is to stress test ToHubTraceListener and TraceToWeb");
            const int count = 1000;//min 100 expected


            Action sequentialTests = () =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                for (int i = 0; i < count; i++)
                {
                    Trace.TraceInformation("Some info from console");
                    Trace.TraceWarning("some warnings.");
                    Trace.TraceError("some errors.");
                    Trace.WriteLine("just WriteLine.");
                    Debug.WriteLine("something in debug");
                }
                stopWatch.Stop();
                Console.WriteLine($"{count * 5} trace messages finished in total seconds: {stopWatch.Elapsed.TotalSeconds} ");
            };

            Trace.TraceInformation("Write the first message, even before the hub is available. Press Enter to continue...");
            Console.ReadLine();
            sequentialTests();

            const int threadCount = 20;
            Action concurrentTest = () =>
            {

                List<Task> tasks = new List<Task>();
                var factory = new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.PreferFairness);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                for (int i = 0; i < count / threadCount; i++)
                {
                    for (int k = 0; k < threadCount; k++)
                    {
                        var t = factory.StartNew(() =>
                        {
                            Trace.TraceInformation("Some info from console");
                            Trace.TraceWarning("some warnings.");
                            Trace.TraceError("some errors.");
                            Trace.WriteLine("just WriteLine.");
                            Debug.WriteLine("something in debug");
                        });
                        tasks.Add(t);
                    }

                    Task.WaitAll(tasks.ToArray());
                }

                stopWatch.Stop();
                var msg = $"---------------------------{count * 5} trace messages in parallel finished in total seconds: {stopWatch.Elapsed.TotalSeconds} ";
                Trace.TraceInformation(msg);
                Console.WriteLine(msg);
            };

            Console.WriteLine("Press Enter for concurrent tests ...");
            for (var i = 0; i < 200; i++)
            {
                Console.ReadLine();
                concurrentTest();
                Console.WriteLine("Press Enter for another round ...");

            }
            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();


        }
    }
}
