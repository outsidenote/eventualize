﻿using System.Diagnostics;
using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
public record EventualizeStreamCursor(string Domain, string StreamType, string StreamId, long Offset = 0) : EventualizeStreamUri(Domain, StreamType, StreamId)
{
    public EventualizeStreamCursor(EventualizeStreamUri streamUri, long offset = 0)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, offset) { }
    public EventualizeStreamCursor(EventualizeAggregate aggregate)
        : this(aggregate.StreamUri.Domain, aggregate.StreamUri.StreamType, aggregate.StreamUri.StreamId, aggregate.LastStoredOffset + 1) { }
}