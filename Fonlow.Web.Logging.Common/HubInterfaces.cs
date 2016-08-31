using System.Collections.Generic;

namespace Fonlow.Diagnostics
{
    public interface ILoggingClient
    {
        void WriteTrace(TraceMessage traceMessage);

        void WriteMessage(string message);

        void WriteTraces(IList<TraceMessage> traceMessages);

        void WriteMessages(string[] messages);
    }

    public interface ILogging
    {
        void UploadTrace(TraceMessage traceMessage);

        void UploadTraces(IList<TraceMessage> traceMessages);

        ClientInfo[] GetAllClients();

        void ReportClientType(ClientType clientType);

        ClientSettings RetrieveClientSettings();
    }


}