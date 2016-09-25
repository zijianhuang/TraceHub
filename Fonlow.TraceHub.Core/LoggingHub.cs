using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Fonlow.TraceHub
{
    [TraceHubAuthorize(Roles = "API")]
    [Microsoft.AspNet.SignalR.Hubs.HubName("loggingHub")]
    [CLSCompliantAttribute(false)]
    public class LoggingHub : Hub<ILoggingClient>, ILogging  //multiple instances expected each time it needs to handle a Hub operation
    {
        #region Hub housekeeping

        public override Task OnConnected()
        {
            RegisterClient();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            ClientInfo clientInfo;
            ClientsDic.Instance.Remove(Context.ConnectionId, out clientInfo);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            RegisterClient();
            return base.OnReconnected();
        }
       
        void RegisterClient()
        {
            Debug.WriteLine($"OnConnected:   ConnectionId: {Context.ConnectionId}; UserIdentityName: {Context.User.Identity.Name}; Client IP address: {GetRemoteIpAddress()}; ");
            ClientsDic.Instance.Add(Context.ConnectionId, new ClientInfo
            {
                Id = Context.ConnectionId,
                ConnectedTimeUtc = DateTime.UtcNow,
                Username = Context.User.Identity.Name,
                IpAddress = GetRemoteIpAddress(),
                UserAgent = Context.Request.Headers["User-Agent"],
            });
        }

        #endregion

        string GetRemoteIpAddress()
        {
            var obj = Context.Request.Environment["server.RemoteIpAddress"];
            return obj as string;
        }

        bool NotAllowed(bool reportError=true)
        {
            if (HubSettings.Instance.ClientCallRestricted)
            {
                var ipAddress = GetRemoteIpAddress();
                if (ipAddress == "::1")  //Reasonable to always allow local call, regardless of the setting
                    return false;

                if (String.IsNullOrWhiteSpace(ipAddress)
                    || !HubSettings.Instance.AllowedToCallServer(ipAddress))
                {
                    if (reportError)
                    {
                        Report(TraceEventType.Warning, $"Client calls are restricted, but this address {ipAddress} tries to call.");
                    }

                    return true;
                }
            }


            return false;
        }

        void Report(TraceEventType eventType, string message)
        {
            var traceMessage = new TraceMessage()
            {
                Message = message,
                EventType = eventType,
                Source = "TraceHub",
                Origin = "TraceHub",
                TimeUtc = DateTime.UtcNow,
            };

            Clients.All.WriteTrace(traceMessage);
        }

        #region ILogging

        public void UploadTrace(TraceMessage traceMessage)
        {
            if (traceMessage == null)
                return;

            if (NotAllowed())
                return;

          //  Clients.All.WriteTrace(traceMessage);
          LoggingHubContext.Instance.Pend(traceMessage);
        }

        public void UploadTraces(IList<TraceMessage> traceMessages)//SignalR server functions does not like array, according to https://github.com/SignalR/SignalR/issues/2672
        {
            if (traceMessages == null)
                return;

            if (NotAllowed())
                return;

          //  Clients.All.WriteTraces(traceMessages);
          LoggingHubContext.Instance.Pend(traceMessages);
        }

        public IList<ClientInfo> GetAllClients()
        {
            if (NotAllowed(false))
                return null;

            var r = ClientsDic.Instance.GetAllClients();
            //Report(TraceEventType.Information, String.Join(Environment.NewLine, r.Select(d => d.ToString())));
            return r;
        }

        public void ReportClientType(ClientType clientType)
        {
            if (NotAllowed())
                return;

            Report(TraceEventType.Information, $"Connection {Context.ConnectionId} is reporting as {clientType}.");
            ClientsDic.Instance.UpdateClientType(Context.ConnectionId, clientType);
            Trace.TraceInformation($"Client reported as {clientType} from IP address {GetRemoteIpAddress()}.");
        }

        public ClientSettings RetrieveClientSettings()
        {
            return HubSettings.Instance.ClientSettings;
        }

        public void ReportClientTypeAndTraceTemplate(ClientType clientType, string template, string origin)
        {
            if (NotAllowed())
                return;

            ClientsDic.Instance.UpdateClientTypeAndTemplate(Context.ConnectionId, clientType, template, origin);
        }

        #endregion
    }





}