using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbSnapshotCursor(string Domain, string Partition, string StreamId, string ViewName, long Offset = 0) : EvDbViewAddress(Domain, Partition, StreamId, ViewName)
{
    public static readonly EvDbSnapshotCursor Empty = new EvDbSnapshotCursor("N/A", "N/A", "N/A", "N/A", -1);

    public EvDbSnapshotCursor(EvDbStreamAddress streamAddress, string viewName, long offset = 0)
        : this(streamAddress.Domain, streamAddress.Partition, streamAddress.StreamId, viewName, offset) { }

    public static bool operator ==(EvDbSnapshotCursor cursor, EvDbViewAddress id)
    {
        EvDbViewAddress cursorId = cursor;
        return cursorId == id;
    }
    public static bool operator !=(EvDbSnapshotCursor cursor, EvDbViewAddress id)
    {
        EvDbViewAddress cursorId = cursor;
        return !(cursor == id);
    }

    public override string ToString()
    {
        return base.ToString() + $"/{Offset}";
    }
}
