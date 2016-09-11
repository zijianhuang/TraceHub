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
    /// <summary>
    /// Double buffers for pending and sending traces in batch.
    /// The sending buffer could ensure the bufferred traces not exceed the 64 KB limit of SignalR. 
    /// </summary>
    internal class QueueBuffer
    {
        ConcurrentQueue<TraceMessage> pendingQueue;
        List<TraceMessage> sendingBuffer;

        public QueueBuffer()
        {
            pendingQueue = new ConcurrentQueue<TraceMessage>();
            sendingBuffer = new List<TraceMessage>();
        }

        const int maxBufferedTraces = 100000;

        public void Pend(TraceMessage tm)
        {
            if (pendingQueue.Count >= maxBufferedTraces)
                return;

            pendingQueue.Enqueue(tm);
        }


        /// <summary>
        /// True if all sent or nothing to sent
        /// </summary>
        /// <param name="loggingConnection"></param>
        /// <returns></returns>
        public QueueStatus SendAll(Func<IList<TraceMessage>, Task> sendTraceMessagesTask)
        {
            if (pendingQueue.IsEmpty && sendingBuffer.Count == 0)
                return QueueStatus.Empty;

            while (!pendingQueue.IsEmpty || sendingBuffer.Count>0)
            {
                if (SendSome(sendTraceMessagesTask) == QueueStatus.Failed)
                    return QueueStatus.Failed;
            }

            return QueueStatus.Sent;
        }

        int totalEstimatedSize = 0;

        QueueStatus SendSome(Func<IList<TraceMessage>, Task> sendTraceMessagesTask)
        {
            TraceMessage tm;
            while ((totalEstimatedSize < Fonlow.TraceHub.Constants.TransportBufferSize) && pendingQueue.TryPeek(out tm))
            {
                var estimatedSize = GetTraceMessageSize(tm);
                if (totalEstimatedSize + estimatedSize >= Fonlow.TraceHub.Constants.TransportBufferSize)
                {
                    break;
                }

                pendingQueue.TryDequeue(out tm);
                sendingBuffer.Add(tm);
                totalEstimatedSize += GetTraceMessageSize(tm);
            }

            if (sendingBuffer.Count > 0)
            {
                try
                {
                    var task = sendTraceMessagesTask(sendingBuffer);// loggingConnection.Invoke("UploadTraces", sendingBuffer);
                    if (task != null)
                    {
                        task.Wait(10000);
                    }
                    else
                    {
                        return QueueStatus.Failed;
                    }
#if DEBUG
                    Console.WriteLine($"{sendingBuffer.Count} traces sent, size: {totalEstimatedSize}");
#endif
                    sendingBuffer.Clear();
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.ToString());
                    return QueueStatus.Failed;
                }

            }
            else
            {
                return QueueStatus.Empty;
            }

            totalEstimatedSize = 0;
            return QueueStatus.Sent;
        }

        int GetTraceMessageSize(TraceMessage tm)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(tm).Length;//not the most time saving way, but the simplest. And this step yields no sigificant overhead to performance.
        }

    }

    public enum QueueStatus
    {
        Empty, Sent, Failed
    }
}
