using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NetTools;

namespace Fonlow.TraceHub
{
    public sealed class HubSettings
    {
        private static readonly Lazy<HubSettings> lazy =
            new Lazy<HubSettings>(() => new HubSettings());

        public static HubSettings Instance { get { return lazy.Value; } }

        private HubSettings()
        {
            var rangesText= System.Configuration.ConfigurationManager.AppSettings["loggingHub_AllowedIpAddresses"];
            AllowedIpAddresses = IPAddressRangesHelper.ParseIPAddressRanges(rangesText);
        }

        /// <summary>
        /// A CSV of IP addresses, ranges and subnet. When this is not null or empty, only connections from these IP addresses will be allowed to call Hub server functions
        /// </summary>
        /// <remarks>To restrict client connections like Web browsers, you have better to do the restrictions on the Web server like IIS.</remarks>
        public IPAddressRange[] AllowedIpAddresses { get; private set; }

        public bool ClientCallRestricted
        {
            get
            {
                return AllowedIpAddresses != null;
            }
        }

        public bool AllowedToCallServer(string ipAddress)
        {
            if (AllowedIpAddresses==null)
            {
                return true;
            }

            return AllowedIpAddresses.IsInRanges(ipAddress);
        }

  
    }
}
