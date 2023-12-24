using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Generator.Equals;

namespace Eventualize.Core.Abstractions.Stream;

[Equatable]
public partial record EventualizeStreamAddress(string Domain, string StreamType, string StreamId):EventualizeStreamBaseAddress(Domain, StreamType)
{
    public EventualizeStreamAddress(EventualizeStreamBaseAddress baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.StreamType, streamId)
    {
    }

    public override string ToString()
    {
        return $"{Domain}/{StreamType}/{StreamId}";
    }
}