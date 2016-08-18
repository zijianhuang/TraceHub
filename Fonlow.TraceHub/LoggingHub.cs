using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Fonlow.TraceHub.Security;

namespace Fonlow.Web.Logging
{
    [Microsoft.AspNet.SignalR.Authorize(Roles = RoleConstants.Api)]
    [Microsoft.AspNet.SignalR.Hubs.HubName("loggingHub")]
    [CLSCompliantAttribute(false)]
    public class LoggingHub : Hub<ILoggingClient>, ILogging  //multiple instances expected each time it needs to handle a Hub operation
    {
        public void UploadTrace(TraceMessage traceMessage)
        {
            if (traceMessage == null)
                return;
            Clients.All.WriteTrace(traceMessage);
          // LoggingHubContext.Instance.Pend(traceMessage);
        }

        public void UploadTraces(IList<TraceMessage> traceMessages)//SignalR server functions does not like array, according to https://github.com/SignalR/SignalR/issues/2672
        {
            if (traceMessages == null)
                return;

             Clients.All.WriteTraces(traceMessages);
           // LoggingHubContext.Instance.Pend(traceMessages);
        }


    }

    /// <summary>
    /// A singleton to access the hub created by runtime.
    /// </summary>
    public sealed class LoggingHubContext : IDisposable
    {
        private static readonly Lazy<LoggingHubContext> lazy =
            new Lazy<LoggingHubContext>(() => new LoggingHubContext());

        public static LoggingHubContext Instance { get { return lazy.Value; } }

        Timer timer;

        private LoggingHubContext()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<LoggingHub, ILoggingClient>();
            pendingQueue = new ConcurrentQueue<TraceMessage>();
            timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }

        void TimerCallback(Object stateInfo)
        {
            SendAll();
            timer.Change(1000, Timeout.Infinite);
        }


        [CLSCompliantAttribute(false)]
        public IHubContext<ILoggingClient> HubContext { get; private set; }

        ConcurrentQueue<TraceMessage> pendingQueue;

        public void Pend(TraceMessage tm)
        {
            pendingQueue.Enqueue(tm);
        }

        public void Pend(TraceMessage[] tms)
        {
            foreach (var tm in tms)
            {
                pendingQueue.Enqueue(tm);
            }
        }

        public bool SendAll()
        {
            TraceMessage tm;
            while (!pendingQueue.IsEmpty)
            {
                pendingQueue.TryPeek(out tm);
                try
                {
                    HubContext.Clients.All.WriteTrace(tm);
                    pendingQueue.TryDequeue(out tm);
                }
                catch (AggregateException)
                {
                    return false;
                }
            }

            return true;
        }

        #region IDisposable pattern
        bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    timer.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }




}