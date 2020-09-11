using NetTools;
using System;
using System.Linq;

namespace Fonlow.TraceHub
{
    public static class IPAddressRangesHelper
    {
        /// <summary>
        /// Return null if no rangesText defined. or throw FormatException if rangeText is invalid
        /// </summary>
        /// <param name="rangesText"></param>
        /// <returns>IPAddress Range array</returns>
        public static IPAddressRange[] ParseIPAddressRanges(string rangesText)
        {
            if (String.IsNullOrWhiteSpace(rangesText))
                return null;

            string[] ss = rangesText.Split(',');
            return ss.Select(s =>
            {
                try
                {
                    return IPAddressRange.Parse(s);
                }
                catch (FormatException)
                {
                    System.Diagnostics.Trace.TraceError($"{s} is not a valid IP address or range.");
                    throw;
                }
            }).Where(d => d != null).ToArray();


        }

        public static bool IsInRanges(this IPAddressRange[] AllowedIpAddresses, string ipAddress)
        {
            return AllowedIpAddresses.Any(d => d.Contains(System.Net.IPAddress.Parse(ipAddress)));
        }
    }
}
