using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbStreamCursor(string Domain, string StreamType, string StreamId, long Offset = 0) : EvDbStreamUri(Domain, StreamType, StreamId)
{
    public EvDbStreamCursor(EvDbStreamUri streamUri, long offset = 0)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, offset) { }
    public EvDbStreamCursor(EvDbAggregate aggregate)
        : this(aggregate.StreamUri.Domain, aggregate.StreamUri.StreamType, aggregate.StreamUri.StreamId, aggregate.LastStoredOffset + 1) { }
}
