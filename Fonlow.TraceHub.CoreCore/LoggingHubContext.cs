using System;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Fonlow.TraceHub
{
    /// <summary>
    /// A singleton to access the hub created by runtime.
    /// </summary>
    public sealed class LoggingHubContext : IDisposable
    {
        public LoggingHubContext(IHubClients<ILoggingClient> hubClients, HubSettings hubSettings)
        {
            HubClients = hubClients;
            this.hubSettings = hubSettings;
            pendingQueue = new PriorityQueueBuffer();
            timer = new Timer(TimerCallback, null, hubSettings.QueueInterval, Timeout.Infinite);
        }

        HubSettings hubSettings;

        Timer timer;

        void TimerCallback(Object stateInfo)
        {
            SendAll().Wait();
            timer.Change(hubSettings.QueueInterval, Timeout.Infinite);
        }


        public IHubClients<ILoggingClient> HubClients { get; private set; }

        PriorityQueueBuffer pendingQueue;

        public void Pend(TraceMessage tm)
        {
            pendingQueue.Pend(tm);
        }

        public void Pend(IList<TraceMessage> tms)
        {
            pendingQueue.Pend(tms);
        }

        public async Task<QueueStatus> SendAll()
        {
            var allowedToRead = ClientsDic.Instance.GetConnectionsAllowedToRead();
            return await pendingQueue.SendAll(async (tms) => await HubClients.Clients(allowedToRead).WriteTraces(tms));
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
