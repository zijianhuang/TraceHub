using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Fonlow.TraceHub
{
    public sealed class HubSettings
    {
        private static readonly Lazy<HubSettings> lazy =
            new Lazy<HubSettings>(() => new HubSettings());

        public static HubSettings Instance { get { return lazy.Value; } }

        private HubSettings()
        {
            AllowedIpAddresses= System.Configuration.ConfigurationManager.AppSettings["loggingHub_AllowedIpAddresses"];
        }

        /// <summary>
        /// A CSV of IP addresses. When this is not null or empty, only connections from these IP addresses will be allowed to call Hub server functions
        /// </summary>
        /// <remarks>To restrict client connections like Web browsers, you have better to do the restrictions on the Web server like IIS. In the future, ip address range may be supported.</remarks>
        public string AllowedIpAddresses { get; private set; }

        public bool ClientCallRestricted
        {
            get
            {
                return !String.IsNullOrWhiteSpace(AllowedIpAddresses);
            }
        }

        public bool AllowedToCallServer(string ipAddress)
        {
            if (String.IsNullOrWhiteSpace(AllowedIpAddresses))
            {
                return true;
            }

            return AllowedIpAddresses.Contains(ipAddress);
        }
    }
}
