using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fonlow.Diagnostics
{
    /// <summary>
    /// Client side functions of hub
    /// </summary>
    public interface ILoggingClient
    {
        Task WriteTrace(TraceMessage traceMessage);

        Task WriteMessage(string message);

        Task WriteTraces(IList<TraceMessage> traceMessages);

        Task WriteMessages(string[] messages);
    }

    /// <summary>
    /// Server side function of the signalR hub, called by a signalR client
    /// </summary>
    public interface ILogging
    {
        void UploadTrace(TraceMessage traceMessage);

        void UploadTraces(IList<TraceMessage> traceMessages);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IList<ClientInfo> GetAllClients();

        /// <summary>
        /// Reports its type after the initial connection is done
        /// </summary>
        /// <param name="clientType"></param>
        void ReportClientType(ClientType clientType);

        void ReportClientTypeAndTraceTemplate(ClientType clientType, string template, string origin);

        /// <summary>
        /// Retrieve client settings generally stored in Web.config. It is up to the client to hornor the settings.
        /// </summary>
        /// <returns></returns>
        ClientSettings RetrieveClientSettings();
    }


}