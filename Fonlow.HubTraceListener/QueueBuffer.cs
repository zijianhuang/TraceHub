using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Essential.Diagnostics;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using System.Collections.Concurrent;

namespace Fonlow.Diagnostics
{
    internal class QueueBuffer
    {
        ConcurrentQueue<TraceMessage> pendingQueue;

        public QueueBuffer()
        {
            pendingQueue = new ConcurrentQueue<TraceMessage>();
        }

        public void Pend(TraceMessage tm)
        {
            if (pendingQueue.Count >= 100000)
                return;

            pendingQueue.Enqueue(tm);
        }

        public bool SendAll(LoggingConnection loggingConnection)
        {
            TraceMessage tm;
            while (!pendingQueue.IsEmpty)
            {
                pendingQueue.TryPeek(out tm);
                try
                {
                    var task = loggingConnection.Invoke("UploadTrace", tm);
                    if (task != null)
                    {
                        task.Wait(100);
                        pendingQueue.TryDequeue(out tm);
                        //And continue
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (AggregateException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
