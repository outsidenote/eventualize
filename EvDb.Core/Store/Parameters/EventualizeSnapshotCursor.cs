using EvDb.Core;
using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbSnapshotCursor(string Domain, string StreamType, string StreamId, string AggregateType, long Offset = 0) : EvDbSnapshotUri(Domain, StreamType, StreamId, AggregateType)
{
    public static readonly EvDbSnapshotCursor Empty = new EvDbSnapshotCursor("N/A", "N/A", "N/A", "N/A", -1);

    public EvDbSnapshotCursor(EvDbStreamUri streamUri, string aggregateType, long offset = 0)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, aggregateType, offset) { }
    public EvDbSnapshotCursor(EvDbAggregate aggregate)
        : this(
            aggregate.StreamUri,
            aggregate.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count
        )
    { }

    public static bool operator ==(EvDbSnapshotCursor cursor, EvDbSnapshotUri uri)
    {
        EvDbSnapshotUri cursorUri = cursor;
        return cursorUri == uri;
    }
    public static bool operator !=(EvDbSnapshotCursor cursor, EvDbSnapshotUri uri)
    {
        EvDbSnapshotUri cursorUri = cursor;
        return !(cursor == uri);
    }

    public override string ToString()
    {
        return base.ToString() + $"/{Offset}";
    }
}
