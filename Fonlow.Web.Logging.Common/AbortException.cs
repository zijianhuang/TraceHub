using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fonlow.Diagnostics
{
    [Serializable]
    public class AbortException : Exception
    {
        public AbortException()
        {
        }

        public AbortException(string message)
            : base(message)
        {
        }

        public AbortException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AbortException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {

        }
    }
}
