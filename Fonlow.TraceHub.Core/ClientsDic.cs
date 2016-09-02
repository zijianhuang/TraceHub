using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using Fonlow.Diagnostics;
using System.Diagnostics;

namespace Fonlow.TraceHub
{
    internal class ClientsDic
    {
        ConcurrentDictionary<string, ClientInfo> dic;

        private static readonly Lazy<ClientsDic> lazy =  new Lazy<ClientsDic>(() => new ClientsDic());

        public static ClientsDic Instance { get { return lazy.Value; } }

        private ClientsDic()
        {
            dic = new ConcurrentDictionary<string, ClientInfo>();
        }

        public void Add(string id, ClientInfo clientInfo)
        {
            dic.TryAdd(id, clientInfo);
        }

        public bool Remove(string id, out ClientInfo clientInfo)
        {
            return dic.TryRemove(id, out clientInfo);
        }

        public void UpdateClientType(string id, ClientType clientType)
        {
            var clientInfo = dic[id];
            Debug.Assert(clientInfo != null, "OnConnected should have already create the item.");
            clientInfo.ClientType = clientType;
        }

        public void UpdateTemplate(string id, string template)
        {
            var clientInfo = dic[id];
            Debug.Assert(clientInfo != null, "OnConnected should have already create the item.");
            if (clientInfo.ClientType!= ClientType.TraceListener)
            {
                Trace.TraceWarning($"This silly client {clientInfo.Id} at {clientInfo.IpAddress} with type {clientInfo.ClientType} not a trace listener tries to report the template");
                return;
            }

            clientInfo.Template = template;
        }

        public IList<ClientInfo> GetAllClients()
        {
            return dic.Values.ToList();
        }
    }

}
