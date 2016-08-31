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

    [DataContract]
    public class ClientInfo
    {
        /// <summary>
        /// GUID string initiated by a client
        /// </summary>
        [DataMember(Name ="id")]
        public string Id { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "ipAddress")]
        public string IpAddress { get; set; }

        [DataMember(Name = "connectedTimeUtc")]
        public DateTime ConnectedTimeUtc { get; set; }

        /// <summary>
        /// Reported by a client
        /// </summary>
        [DataMember(Name = "clientType")]
        public ClientType ClientType { get; set; }

        [DataMember(Name ="userAgent")]
        public string UserAgent { get; set; }

        public override string ToString()
        {
            return $"HubClient  Id: {Id}; IP Address: {IpAddress}; Connected UTC: {ConnectedTimeUtc}; User: {Username}; Type: {ClientType}; UserAgent: {UserAgent}";
        }
    }

    [Flags]
    [DataContract]
    public enum ClientType
    {
        [EnumMember]
        Undefined=0,
        [EnumMember]
        TraceListener = 1,
        [EnumMember]
        Browser = 2,
        [EnumMember]
        Console = 4
    }

    /// <summary>
    /// It is up to the client to hornor these settings.
    /// </summary>
    [DataContract]
    public class ClientSettings
    {
        [DataMember(Name = "bufferSize")]
        public int BufferSize;

        [DataMember(Name = "advancedMode")]
        public bool AdvancedMode;


    }

}
