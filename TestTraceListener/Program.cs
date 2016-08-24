using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace TestTraceListener
{
    class Program
    {

        const int count = 500;//min 100 expected

        static void Main(string[] args)
        {
            Console.WriteLine("This program is to stress test ToHubTraceListener and TraceToWeb. ");



            while(true)
            {
                Console.WriteLine("Press O and enter to trace 1, T to trace 10, S to trace sequential 5000, C to trace concurrent 5000, and Q to quit ");
                var r = Console.ReadLine();
                r = r.ToUpper();
                switch (r)
                {
                    case "O":
                        OneLineTest();
                        break;
                    case "T":
                        TraceTest();
                        break;
                    case "S":
                        SequentialTests();
                        break;
                    case "C":
                        ConcurrentTests();
                        break;
                    case "Q":
                        Environment.Exit(0);
                        break;
                    case "R":
                        RandomTestsNeverEnd();
                        break;
                    default:
                        break;
                }
            }

        }

        static readonly TraceSource myAppSource = new TraceSource("myAppSource");


        static void TraceSourceTest()
        {
            Trace.TraceInformation("TraceEventType.Information is for informational message.");
            myAppSource.TraceEvent(TraceEventType.Warning, 0, "TraceEvent with TraceEventType.Warning is for writing warning information to the trace listeners in the Listeners collection.");
            Trace.TraceError("TraceEventType.Error is to identify recoverable error.");
            Trace.WriteLine("Trace.WriteLine with TraceEventType.Verbose is for debugging trace.");
            myAppSource.TraceEvent(TraceEventType.Critical, 1234, "TraceEventType.Critical is for fatal error or application crash");
            myAppSource.TraceEvent(TraceEventType.Resume, 123, "TraceEventType.Resume is for resumption ofa logical operation.");
            myAppSource.TraceEvent(TraceEventType.Start, 11, "TraceEventType.Start is for starting of a logical operation.");
            myAppSource.TraceEvent(TraceEventType.Stop, 12, "TraceEventType.Stop is for stopping of a logical operation.");
            myAppSource.TraceEvent(TraceEventType.Suspend, 13, "TraceEventType.Suspend is for suspension of a logical oepration.");
            myAppSource.TraceEvent(TraceEventType.Transfer, 14, "TraceEventType.Transfer is for changing of correlation identity.");

        }

        static void TraceTest()
        {
            Trace.TraceInformation("Structured tracing is available since .NET Framework 2 through TraceSource and TraceEventCache etc. Structured logging is done through TraceListener derived classes, include thos in Essential Diagnostics.");
            Trace.TraceWarning("some warnings.");
            Trace.TraceError("some errors.");
            Trace.WriteLine("just WriteLine.");
            Debug.WriteLine("Debug.Writeline TraceEventType.Verbose is for debugging trace in the debug build.");
            myAppSource.TraceEvent(TraceEventType.Warning, 0, "TraceEvent with TraceEventType.Warning is for writing warning information to the trace listeners in the Listeners collection.");
            Trace.TraceError("TraceEventType.Error is to identify recoverable error.");
            Trace.WriteLine("Trace.WriteLine with TraceEventType.Verbose is for debugging trace.");
            myAppSource.TraceEvent(TraceEventType.Critical, 1234, "TraceEventType.Critical is for fatal error or application crash");
            myAppSource.TraceEvent(TraceEventType.Resume, 123, "TraceEventType.Resume is for resumption ofa logical operation.");
        }

        static void OneLineTest()
        {
            Trace.TraceInformation("Structured tracing is available since .NET Framework 2 through TraceSource and TraceEventCache etc. Structured logging is done through TraceListener derived classes, include thos in Essential Diagnostics.");
        }

        static void SequentialTests()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < count; i++)
            {
                TraceTest();
            }
            stopWatch.Stop();
            Console.WriteLine($"{count * 10} trace messages finished in total seconds: {stopWatch.Elapsed.TotalSeconds} ");
        }


        static void ConcurrentTests()
        {
            const int threadCount = 20;

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
                        TraceSourceTest();
                    });
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }

            stopWatch.Stop();
            var msg = $"---------------------------{count * 10} trace messages in parallel finished in total seconds: {stopWatch.Elapsed.TotalSeconds} ";
            Trace.TraceInformation(msg);
            Console.WriteLine(msg);
        }

        static Timer timer;

        static void RandomTestsNeverEnd()
        {
            timer = new Timer((stateInfo) =>
            {
                TraceSourceTest();
                timer.Change(1000, Timeout.Infinite);
            },
            null, 1000, Timeout.Infinite);

        }


    }
}
