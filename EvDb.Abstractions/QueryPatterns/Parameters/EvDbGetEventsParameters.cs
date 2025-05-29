// Ignore Spelling: Occ

namespace EvDb.Core.Adapters.Internals;

public readonly record struct EvDbGetEventsParameters
{
    public EvDbGetEventsParameters(
                                EvDbStreamCursor cursor,
                                int batchSize = 300)
    {
        StreamType = cursor.StreamType;
        StreamId = cursor.StreamId;
        BatchSize = batchSize;
        SinceOffset = cursor.Offset;
    }

    public EvDbStreamTypeName StreamType { get; }
    public string StreamId { get; }
    public int BatchSize { get; }
    public long SinceOffset { get; init; }

    public EvDbGetEventsParameters ContinueFrom(EvDbEvent? last)
    {
        if (last == null)
            return this;

        var parameters = this with { SinceOffset = last.Value.StreamCursor.Offset + 1 };
        return parameters;
    }
}
