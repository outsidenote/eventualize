using System.Diagnostics;
using Eventualize.Core;
using Generator.Equals;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EventualizeSnapshotCursor(string Domain, string StreamType, string StreamId, string AggregateType, long Offset = 0) : EventualizeSnapshotUri(Domain, StreamType, StreamId, AggregateType)
{
    public EventualizeSnapshotCursor(EventualizeStreamUri streamUri, string aggregateType, long offset = 0)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, aggregateType, offset) { }
    public EventualizeSnapshotCursor(EventualizeAggregate aggregate)
        : this(
            aggregate.StreamUri,
            aggregate.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count
        )
    { }

    public static bool operator ==(EventualizeSnapshotCursor cursor, EventualizeSnapshotUri uri)
    {
        EventualizeSnapshotUri cursorUri = cursor;
        return cursorUri == uri;
    }
    public static bool operator !=(EventualizeSnapshotCursor cursor, EventualizeSnapshotUri uri)
    {
        EventualizeSnapshotUri cursorUri = cursor;
        return !(cursor == uri);
    }

    public override string ToString()
    {
        return base.ToString() + $"/{Offset}";
    }
}
