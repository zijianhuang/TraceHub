using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Fonlow.TraceHub
{
    /// <summary>
    /// A singleton to access the hub created by runtime.
    /// </summary>
    public sealed class LoggingHubContext : IDisposable
    {
        #region Singleton
        private static readonly Lazy<LoggingHubContext> lazy = new Lazy<LoggingHubContext>(() => new LoggingHubContext());

        public static LoggingHubContext Instance { get { return lazy.Value; } }

        private LoggingHubContext()
        {
            HubContext = GlobalHost.ConnectionManager.GetHubContext<LoggingHub, ILoggingClient>();
            pendingQueue = new PriorityQueueBuffer();
            timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }
        #endregion


        Timer timer;

        void TimerCallback(Object stateInfo)
        {
            SendAll();
            timer.Change(1000, Timeout.Infinite);
        }


        [CLSCompliantAttribute(false)]
        public IHubContext<ILoggingClient> HubContext { get; private set; }

        PriorityQueueBuffer pendingQueue;

        public void Pend(TraceMessage tm)
        {
            pendingQueue.Pend(tm);
        }

        public void Pend(IList<TraceMessage> tms)
        {
            pendingQueue.Pend(tms);
        }

        public QueueStatus SendAll()
        {
            return pendingQueue.SendAll((tms) => HubContext.Clients.All.WriteTraces(tms));
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
