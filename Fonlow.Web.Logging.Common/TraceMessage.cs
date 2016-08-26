using System;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Fonlow.Diagnostics
{
    [DataContract]
    public class TraceMessage
    {
        #region Trace parameters
        [DataMember(Name = "eventType")]
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
        public DateTime TimeUtc { get; set; }
        #endregion


        [DataMember(Name = "origin")]
        public string Origin { get; set; }

    }
}
