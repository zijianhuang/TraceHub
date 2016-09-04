using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Essential.Diagnostics;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;

namespace Fonlow.Diagnostics
{
    public class HubTraceListener : TraceListenerBase
    {
        TraceFormatter traceFormatter = new TraceFormatter();


        Lazy<LoggingConnection> _loggingConnection;

        Timer timer;

        QueueBuffer queueBuffer;

        public HubTraceListener()
        {
            _loggingConnection = new Lazy<LoggingConnection>(()=>new LoggingConnection(this), true);
            queueBuffer = new QueueBuffer();
            timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }


        void TimerCallback(Object stateInfo)
        {
            var hubInfo = new HubInfo
            {
                Url = HubUrl,
                User = ApiUser,
                Password = ApiPassword,
                Key = ApiKey,
            };

            loggingConnection.ExecuteOnceAsync(hubInfo);
#if DEBUG
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif
            queueBuffer.SendAll(loggingConnection);

#if DEBUG
            watch.Stop();
            if (watch.ElapsedMilliseconds > 1)
            {
                Console.WriteLine("Send all in milliseconds: " + watch.ElapsedMilliseconds);
            }
#endif

            timer.Change(1000, Timeout.Infinite);// 1 second is a good number, with optimal performance, 0.5 second does not make performance noticablly better.
        }

        LoggingConnection loggingConnection
        {
            get
            {
                return _loggingConnection.Value;
            }
        }


        protected override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, Guid? relatedActivityId, object[] data)
        {
            var output = String.IsNullOrEmpty(Template) ? message : traceFormatter.Format(Template,
                            this,
                            eventCache,
                            source,
                            eventType,
                            id,
                            message,
                            relatedActivityId,
                            data
                            );

            queueBuffer.Pend(new TraceMessage()
            {
                EventType = eventType,
                TimeUtc = eventCache == null ? DateTime.UtcNow : eventCache.DateTime.ToUniversalTime(),
                Message = output,
                Callstack = IncludeCallstack ? eventCache.Callstack : null,
                Id = id,
                ProcessId = eventCache == null ? new Nullable<int>() : eventCache.ProcessId,
                RelatedActivityId = relatedActivityId,
                Source = source,
                ThreadId = eventCache == null ? null : eventCache.ThreadId,

                Origin = InstanceId,
            });
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        private static readonly string[] _supportedAttributes = new string[] { "template", "hubUrl", "instanceId", "callstack", "apiUser", "apiPassword", "apiKey" };

        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }

        private const string _defaultTemplate = "{DateTime:u} [{Thread}] {EventType} {Source} {Id}: {Message}{Data}";

        #region Custom Attributes

        public string Template
        {
            get
            {
                if (Attributes.ContainsKey("template"))
                {
                    return Attributes["template"];
                }
                else
                {
                    return _defaultTemplate;
                }
            }
            set
            {
                Attributes["template"] = value;
            }
        }

        public string HubUrl
        {
            get
            {
                return Attributes["hubUrl"];
            }

            set
            {
                Attributes["hubUrl"] = value;
            }

        }

        public string InstanceId
        {
            get
            {
                return Attributes["instanceId"];
            }
            set
            {
                Attributes["instanceId"] = value;
            }
        }

        public string Callstack
        {
            get
            {
                return Attributes["callstack"];
            }
            set
            {
                Attributes["callstack"] = value;
            }
        }

        public string ApiKey
        {
            get
            {
                return Attributes["apiKey"];
            }
            set
            {
                Attributes["apiKey"] = value;
            }
        }

        public string ApiUser
        {
            get
            {
                return Attributes["apiUser"];
            }
            set
            {
                Attributes["apiUser"] = value;
            }
        }

        public string ApiPassword
        {
            get
            {
                return Attributes["apiPassword"];
            }
            set
            {
                Attributes["apiPassword"] = value;
            }
        }

        #endregion

        public bool IncludeCallstack
        {
            get
            {
                return String.Equals("true", Callstack, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (loggingConnection != null)//loggingConnection gets disposed in AppDomain.CurrentDomain.ProcessExit as well.
            {
                loggingConnection.Dispose(); 
            }

            timer.Dispose();//If the host process is not calling Dispose() when shutting down, this won't be a problem.

            base.Dispose(disposing);
        }

    }

}
