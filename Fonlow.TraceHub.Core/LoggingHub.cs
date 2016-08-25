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
    [TraceHubAuthorize(Roles = "API")]
    [Microsoft.AspNet.SignalR.Hubs.HubName("loggingHub")]
    [CLSCompliantAttribute(false)]
    public class LoggingHub : Hub<ILoggingClient>, ILogging  //multiple instances expected each time it needs to handle a Hub operation
    {
        public void UploadTrace(TraceMessage traceMessage)
        {
            if (traceMessage == null)
                return;

            if (NotAllowed())
                return;

            Clients.All.WriteTrace(traceMessage);
          // LoggingHubContext.Instance.Pend(traceMessage);
        }

        public void UploadTraces(IList<TraceMessage> traceMessages)//SignalR server functions does not like array, according to https://github.com/SignalR/SignalR/issues/2672
        {
            if (traceMessages == null)
                return;

            if (NotAllowed())
                return;

             Clients.All.WriteTraces(traceMessages);
           // LoggingHubContext.Instance.Pend(traceMessages);
        }

        bool NotAllowed()
        {
            if (HubSettings.Instance.ClientCallRestricted)
            {
                var ipObj = Context.Request.Environment["server.RemoteIpAddress"];
                var ipAddress = ipObj as string;
                if (ipAddress == "::1")  //Reasonable to always allow local call, regardless of the setting
                    return false;

                if (String.IsNullOrWhiteSpace(ipAddress)
                    || !HubSettings.Instance.AllowedToCallServer(ipAddress))
                {
                    ReportError($"Client calls are restricted, but this address {ipAddress} tries to call.");
                    return true;
                }
            }


            return false;
        }

        void ReportError(string message)
        {
            var traceMessage = new TraceMessage()
            {
                Message = message,
                EventType = TraceEventType.Warning,
                Source = "TraceHub",
                Origin = "TraceHub",
                TimeUtc = DateTime.UtcNow,
            };

            Clients.All.WriteTrace(traceMessage);
        }
    }





}