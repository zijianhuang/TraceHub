using System.Collections.Generic;
using System.Runtime.Serialization;

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
    }


    [DataContract]
    public class TokenResponseModel
    {
        [DataMember(Name ="access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name ="token_type")]
        public string TokenType { get; set; }

        [DataMember(Name ="expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name ="userName")]
        public string Username { get; set; }

        [DataMember(Name =".issued")]
        public string IssuedAt { get; set; }

        [DataMember(Name =".expires")]
        public string ExpiresAt { get; set; }
    }

}