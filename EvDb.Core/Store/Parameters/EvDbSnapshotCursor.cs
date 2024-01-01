using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbSnapshotCursor(string Domain, string EntityType, string EntityId, string AggregateType, long Offset = 0) : EvDbSnapshotId(Domain, EntityType, EntityId, AggregateType)
{
    public static readonly EvDbSnapshotCursor Empty = new EvDbSnapshotCursor("N/A", "N/A", "N/A", "N/A", -1);

    public EvDbSnapshotCursor(EvDbStreamId streamId, string aggregateType, long offset = 0)
        : this(streamId.Domain, streamId.EntityType, streamId.EntityId, aggregateType, offset) { }
    public EvDbSnapshotCursor(EvDbAggregate aggregate)
        : this(
            aggregate.StreamId,
            aggregate.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count
        )
    { }

    public static bool operator ==(EvDbSnapshotCursor cursor, EvDbSnapshotId id)
    {
        EvDbSnapshotId cursorId = cursor;
        return cursorId == id;
    }
    public static bool operator !=(EvDbSnapshotCursor cursor, EvDbSnapshotId id)
    {
        EvDbSnapshotId cursorId = cursor;
        return !(cursor == id);
    }

    public override string ToString()
    {
        return base.ToString() + $"/{Offset}";
    }
}
