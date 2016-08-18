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
        List<TraceMessage> sendingBuffer;

        public QueueBuffer()
        {
            pendingQueue = new ConcurrentQueue<TraceMessage>();
            sendingBuffer = new List<TraceMessage>();
        }

        public void Pend(TraceMessage tm)
        {
            if (pendingQueue.Count >= 100000)
                return;

            pendingQueue.Enqueue(tm);
        }

        public bool SendAll(LoggingConnection loggingConnection)
        {
            while (!pendingQueue.IsEmpty)
            {
                if (!SendSome(loggingConnection))
                    return false;
            }

            return true;
        }

        bool SendSome(LoggingConnection loggingConnection)
        {
            const int max = 100;
            int i = 0;
            TraceMessage tm;
            while ((sendingBuffer.Count<max) &&  pendingQueue.TryDequeue(out tm))
            {
                sendingBuffer.Add(tm);
                i++;
            }

            if (sendingBuffer.Count>0)
            {
                try
                {
                    var task = loggingConnection.Invoke("UploadTraces", sendingBuffer);
                    if (task != null)
                    {
                        task.Wait(10000);
                    }
                    else
                    {
                        return false;
                    }
                    sendingBuffer.Clear();
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }

            }

            return true;
        }
    }
}
