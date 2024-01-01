using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbStreamCursor(string Domain, string EntityType, string EntityId, long Offset = 0) : EvDbStreamId(Domain, EntityType, EntityId)
{
    public EvDbStreamCursor(EvDbStreamId streamId, long offset = 0)
        : this(streamId.Domain, streamId.EntityType, streamId.EntityId, offset) { }
    public EvDbStreamCursor(EvDbAggregate aggregate)
        : this(aggregate.StreamId.Domain, aggregate.StreamId.EntityType, aggregate.StreamId.EntityId, aggregate.LastStoredOffset + 1) { }
}
