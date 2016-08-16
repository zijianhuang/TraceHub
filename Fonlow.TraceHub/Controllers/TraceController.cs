using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Diagnostics;
using Fonlow.Diagnostics;
using Fonlow.TraceHub.Security;

namespace Fonlow.Web.Logging.Controllers
{
    [Authorize(Roles = RoleConstants.Api)]
    [RoutePrefix("api/Trace")]
    public class TraceController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// contentType: 'application/x-www-form-urlencoded; charset=utf-8'
        /// data is =YourMessage
        /// </summary>
        /// <param name="message"></param>
        [HttpPut]
        [Route("message")]
        public void WriteMessage([FromBody] string message)
        {
            LoggingHubContext.Instance.HubContext.Clients.All.WriteMessage(message);
        }

        [HttpPut]
        [Route("messages")]
        public void WriteMessages([FromBody] string[] messages)
        {
            LoggingHubContext.Instance.HubContext.Clients.All.WriteMessages(messages);
        }

        /// <summary>
        /// {
        /// "EventType" : 1024,
        /// "Message" : "abcdefg"
        /// }
        /// </summary>
        /// <param name="traceMessage"></param>
        [HttpPut]
        [Route("traceMessage")]
        public void WriteTraceMessage([FromBody] TraceMessage traceMessage)
        {
            if (traceMessage == null)
                return;

            LoggingHubContext.Instance.Pend(traceMessage);
        }

        [HttpPut]
        [Route("traceMessages")]
        public void WriteTraceMessages([FromBody] TraceMessage[] traceMessages)
        {
            if (traceMessages == null)
                return;

            LoggingHubContext.Instance.HubContext.Clients.All.WriteTraces(traceMessages);
        }
    }


}
