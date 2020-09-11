using System;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Fonlow.TraceHub
{
    [Microsoft.AspNetCore.Authorization.AuthorizeAttribute(Roles = "API")]
//    [Microsoft.AspNetCore.SignalR.Hubs.HubName("loggingHub")]
    [CLSCompliantAttribute(false)]
    public class LoggingHub : Hub<ILoggingClient>, ILogging  //multiple instances expected each time it needs to handle a Hub operation
    {
        public LoggingHub(HubSettings hubSettings)
        {
            this.hubSettings = hubSettings;
            this.connectionsActions = new LoggingHubContext(this.Clients, hubSettings);
        }

        HubSettings hubSettings;

        LoggingHubContext connectionsActions;

        #region Hub housekeeping override

        public override Task OnConnectedAsync()
        {
            RegisterClient();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (exception == null)
            {
                if (ClientsDic.Instance.RemoveByValue(Context.ConnectionId))//It is a conduit with clientType="Medilink".
                {
                    Debug.WriteLine($"SignalR connection {Context.ConnectionId} is disconnected.");
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region ILogging

        public void UploadTrace(TraceMessage traceMessage)
        {
            if (traceMessage == null)
                return;

            if (NotAllowed())
                return;

            // Clients.All.WriteTrace(traceMessage);
            connectionsActions.Pend(traceMessage);
        }

        public void UploadTraces(IList<TraceMessage> traceMessages)//SignalR server functions does not like array, according to https://github.com/SignalR/SignalR/issues/2672
        {
            if (traceMessages == null)
                return;

            if (NotAllowed())
                return;

            //Clients.All.WriteTraces(traceMessages);
            connectionsActions.Pend(traceMessages);
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
            if (NotAllowedToPush(false))
            {
                return new ClientSettings
                {
                    AdvancedMode = false,
                    BufferSize = 200,
                };
            }
            return hubSettings.ClientSettings;
        }

        public void ReportClientTypeAndTraceTemplate(ClientType clientType, string template, string origin)
        {
            if (NotAllowed())
                return;

            ClientsDic.Instance.UpdateClientTypeAndTemplate(Context.ConnectionId, clientType, template, origin);
        }

        #endregion

        void RegisterClient()
        {
            Debug.WriteLine($"OnConnected:   ConnectionId: {Context.ConnectionId}; UserIdentityName: {Context.User.Identity.Name}; Client IP address: {GetRemoteIpAddress()}; ");
            var notAllowedToWrite = NotAllowed(true);
            var notAllowedToRead = NotAllowedToPush(true);
            //if (notAllowedToRead && notAllowedToWrite) Apparently 2.2.1 does not support that server disconnects a client if AuthorizeAttribute is not used.
            //{
            //    this.Dispose(); I hoped this is a workaround, however apparently Dispose does not disconnect. :(
            //    return; Another workaround is to have a push function to ask the client to disconnect.
            //}

            ClientsDic.Instance.Add(Context.ConnectionId, new ClientInfo
            {
                Id = Context.ConnectionId,
                ConnectedTimeUtc = DateTime.UtcNow,
                Username = Context.User.Identity.Name,
                IpAddress = GetRemoteIpAddress(),
                UserAgent = GetUserAgent(),
                Write = !notAllowedToWrite,
                Read=!notAllowedToRead,
            });
        }

        string GetRemoteIpAddress()
        {
            return Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
        }

        string GetUserAgent()
        {
            return Context.GetHttpContext().Request.Headers["User-Agent"].ToString();
        }

        /// <summary>
        /// Not allow to call server function, so the client could not write.
        /// </summary>
        /// <param name="reportError"></param>
        /// <returns></returns>
        bool NotAllowed(bool reportError = true)
        {
            if (hubSettings.ClientCallRestricted)
            {
                var ipAddress = GetRemoteIpAddress();
                if (ipAddress == "::1")  //Reasonable to always allow local call, regardless of the setting
                    return false;

                if (String.IsNullOrWhiteSpace(ipAddress)
                    || !hubSettings.AllowedToCallServer(ipAddress))
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

        /// <summary>
        /// Not allow to push to the client, so the client could not read traces.
        /// </summary>
        /// <param name="reportError"></param>
        /// <returns></returns>
        bool NotAllowedToPush(bool reportError = true)
        {
            if (hubSettings.ClientPushRestricted)
            {
                var ipAddress = GetRemoteIpAddress();
                if (ipAddress == "::1")  //Reasonable to always allow local call, regardless of the setting
                    return false;

                if (String.IsNullOrWhiteSpace(ipAddress)
                    || !hubSettings.AllowedToPush(ipAddress))
                {
                    if (reportError)
                    {
                        Report(TraceEventType.Warning, $"View are restricted, but this address {ipAddress} tries to view.");
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

    }





}