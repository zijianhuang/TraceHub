using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.ServiceModel;

namespace Fonlow.Diagnostics
{
    public interface ILoggingClient
    {
        void WriteTrace(TraceMessage traceMessage);

        void WriteMessage(string message);

        void WriteTraces(TraceMessage[] traceMessages);

        void WriteMessages(string[] messages);
    }

    public interface ILogging
    {
        void UploadTrace(TraceMessage traceMessage);

        void UploadTraces(TraceMessage[] traceMessages);
    }

    [DataContract]
    public class TraceMessage
    {
        #region Trace parameters
        [DataMember(Name ="eventType")]
        public TraceEventType EventType { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "relatedActivityId")]
        public Guid? RelatedActivityId { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        #endregion

        #region TraceEventCache
        [DataMember(Name = "callstack")]
        public string Callstack { get; set; }

        [DataMember(Name = "processiId")]
        public int? ProcessId { get; set; }

        [DataMember(Name = "threadId")]
        public string ThreadId { get; set; }

        /// <summary>
        /// The Time when the trace is created in the source. Assuming all clocks in servers are correct.
        /// </summary>
        [DataMember(Name = "timeUtc")]
        public DateTime? TimeUtc { get; set; }
        #endregion


        [DataMember(Name = "origin")]
        public string Origin { get; set; }

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