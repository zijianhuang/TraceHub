using Fonlow.Diagnostics;
using Microsoft.Extensions.Configuration;
using NetTools;
using System;

namespace Fonlow.TraceHub
{
    public sealed class HubSettings
    {
        private HubSettings(IConfiguration config)
        {
            var appSettings = config.GetSection("appSettings");
            var rangesText = appSettings["loggingHub_AllowedIpAddresses"];
            AllowedIpAddresses = IPAddressRangesHelper.ParseIPAddressRanges(rangesText);

            var rangesTextForView = appSettings["loggingHub_AllowedIpAddressesForView"];
            AllowedIpAddressesForView = IPAddressRangesHelper.ParseIPAddressRanges(rangesTextForView);

            int bufferSize = 2000;
            int.TryParse(appSettings["loggingHub_ClientBufferSize"], out bufferSize);
            if (bufferSize > Constants.ClientBufferSizeMax)
            {
                bufferSize = Constants.ClientBufferSizeMax;
            }
            else if (bufferSize < Constants.ClientBufferSizeMin)
            {
                bufferSize = Constants.ClientBufferSizeMin;
            }

            QueueInterval = 200;

            var s = appSettings["loggingHub_QueueInterval"];
            int si;
            if (int.TryParse(s, out si))
            {
                if (si >= 100 && si <= 2000)
                {
                    QueueInterval = si;
                }
            }

            ClientSettings = new ClientSettings
            {
                AdvancedMode = String.Equals("true", appSettings["loggingHub_AdvancedMode"], StringComparison.CurrentCultureIgnoreCase),
                BufferSize = bufferSize,
            };

        }

        /// <summary>
        /// A CSV of IP addresses, ranges and subnet. When this is not null or empty, only connections from these IP addresses will be allowed to call Hub server functions
        /// </summary>
        /// <remarks>To restrict client connections like Web browsers, you have better to do the restrictions on the Web server like IIS.</remarks>
        public IPAddressRange[] AllowedIpAddresses { get; private set; }

        public IPAddressRange[] AllowedIpAddressesForView { get; private set; }

        public ClientSettings ClientSettings { get; private set; }


        /// <summary>
        /// Default value is 200 ms
        /// </summary>
        public int QueueInterval { get; private set; }

        public bool ClientCallRestricted
        {
            get
            {
                return AllowedIpAddresses != null;
            }
        }

        public bool ClientPushRestricted
        {
            get
            {
                return AllowedIpAddressesForView != null;
            }
        }

        public bool AllowedToCallServer(string ipAddress)
        {
            if (AllowedIpAddresses == null)
            {
                return true;
            }

            return AllowedIpAddresses.IsInRanges(ipAddress);
        }

        public bool AllowedToPush(string ipAddress)
        {
            if (AllowedIpAddressesForView == null)
            {
                return true;
            }

            return AllowedIpAddressesForView.IsInRanges(ipAddress);
        }


    }
}
