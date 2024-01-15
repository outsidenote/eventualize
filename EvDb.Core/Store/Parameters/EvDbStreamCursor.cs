using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
[Equatable]
public partial record EvDbStreamCursor(string Domain, string Partition, string StreamId, long Offset = 0) : EvDbStreamAddress(Domain, Partition, StreamId)
{
    public EvDbStreamCursor(EvDbStreamAddress streamId, long offset = 0)
        : this(streamId.Domain, streamId.Partition, streamId.StreamId, offset) { }
    public EvDbStreamCursor(EvDbPartitionAddress partition, string streamId, long offset = 0)
        : this(partition.Domain, partition.Partition, streamId, offset) { }
    public EvDbStreamCursor(IEvDbAggregate aggregate)
        : this(aggregate.StreamId.Domain, aggregate.StreamId.Partition, aggregate.StreamId.StreamId, aggregate.LastStoredOffset + 1) { }
}
