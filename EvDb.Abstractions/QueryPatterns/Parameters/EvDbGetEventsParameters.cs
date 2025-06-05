// Ignore Spelling: Occ

namespace EvDb.Core.Adapters.Internals;

public readonly record struct EvDbGetEventsParameters
{
    private const int BATCH_SIZE = 300;

    public EvDbGetEventsParameters(
                                EvDbStreamCursor cursor)
    {
        StreamType = cursor.StreamType;
        StreamId = cursor.StreamId;
        BatchSize = BATCH_SIZE;
        SinceOffset = cursor.Offset;
    }

    public string StreamType { get; }
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
