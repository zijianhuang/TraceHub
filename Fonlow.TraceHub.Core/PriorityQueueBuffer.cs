using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Fonlow.Diagnostics;
using Priority_Queue;

namespace Fonlow.TraceHub
{
    /// <summary>
    /// Double buffers for pending and sending traces in batch.
    /// The sending buffer could ensure the bufferred traces not exceed the 64 KB limit of SignalR. 
    /// Use Priority Queue to ensure traces from different nodes with different latency to TraceHub will be pushed to clients in right order by the time stamp, 
    /// as long as the latency won't exceed 1 second, presuming the clocks of these nodes are synchronized with a time server.
    /// </summary>
    internal class PriorityQueueBuffer
    {
        SimplePriorityQueue<TraceMessage> pendingQueue;
        List<TraceMessage> sendingBuffer;

        public PriorityQueueBuffer()
        {
            pendingQueue = new SimplePriorityQueue<TraceMessage>();
            sendingBuffer = new List<TraceMessage>();
        }

        const int maxBufferedTraces = 100000;

        public void Pend(TraceMessage tm)
        {
            if (pendingQueue.Count >= maxBufferedTraces)
                return;

            pendingQueue.Enqueue(tm, tm.TimeUtc.ToOADate());
        }

        public void Pend(IList<TraceMessage> tms)
        {
            if (tms == null)
                return;

            foreach (var item in tms)
            {
                Pend(item);
            }
        }


        /// <summary>
        /// True if all sent or nothing to sent
        /// </summary>
        /// <param name="loggingConnection"></param>
        /// <returns></returns>
        public QueueStatus SendAll(Action<IList<TraceMessage>> sendTraceMessagesTask)
        {
            if (pendingQueue.Count==0 && sendingBuffer.Count == 0)
                return QueueStatus.Empty;

            while (pendingQueue.Count>0 || sendingBuffer.Count > 0)
            {
                if (SendSome(sendTraceMessagesTask) == QueueStatus.Failed)
                    return QueueStatus.Failed;
            }

            return QueueStatus.Sent;
        }

        int totalEstimatedSize = 0;

        QueueStatus SendSome(Action<IList<TraceMessage>> sendTraceMessagesTask)
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
                    sendTraceMessagesTask(sendingBuffer);
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
