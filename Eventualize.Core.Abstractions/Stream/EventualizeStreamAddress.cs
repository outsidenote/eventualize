using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.Abstractions.Stream
{
    public record EventualizeStreamAddress(string Domain, string StreamType, string StreamId)
    {
        public EventualizeStreamAddress(EventualizeStreamBaseAddress baseAddress, string streamId)
            : this(baseAddress.Domain, baseAddress.StreamType, streamId)
        {
        }
    }
}