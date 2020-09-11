using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fonlow.TraceHub
{
    public static class Constants
    {
        public const int ClientBufferSizeMax = 10000;
        public const int ClientBufferSizeMin = 200;
        public const int TransportBufferSize = 64000;// SignalR transport has a 64KB (65536? presumbly) limit. This const here also consider the overhead of JSON.
    }
}
