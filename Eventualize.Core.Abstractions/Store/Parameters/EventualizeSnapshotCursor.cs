using System.Diagnostics;
using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
public record EventualizeSnapshotCursor(string Domain, string StreamType, string StreamId, string AggregateType, long Offset = 0) : EventualizeStreamCursor(Domain, StreamType, StreamId, Offset)
{
    public EventualizeSnapshotCursor(EventualizeStreamUri streamUri, string aggregateType, long offset = 0)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, aggregateType, offset) { }
    public EventualizeSnapshotCursor(EventualizeAggregate aggregate)
        : this(
            aggregate.StreamUri.Domain,
            aggregate.StreamUri.StreamType,
            aggregate.StreamUri.StreamId,
            aggregate.AggregateType,
            aggregate.LastStoredOffset+aggregate.PendingEvents.Count
        ) { }
}
