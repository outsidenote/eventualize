// Ignore Spelling: Occ

using System.Data;

namespace EvDb.Core.Adapters.Internals;

public readonly record struct EvDbGetMessagesParameters
{
    private const int BATCH_SIZE = 300;

    public EvDbGetMessagesParameters(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions options)
    {
        Channels = filter.Channels?.Any() == true
                        ? filter.Channels.Select(m => m.Value).ToArray()
                        : [];
        MessageTypes = filter.MessageTypes?.Any() == true
                        ? filter.MessageTypes.Select(m => m.Value).ToArray()
                        : [];
        SinceDate = filter.Since;
    }

    public int BatchSize { get; } = BATCH_SIZE; // Default batch size for fetching messages

    public string[] Channels { get; }

    public string[] MessageTypes { get; }

    public DateTimeOffset SinceDate { get; init; }

    public EvDbGetMessagesParameters ContinueFrom(EvDbMessage? last)
    {
        if (last == null)
            return this;

        if (!last.Value.StoredAt.HasValue)
            throw new InvalidOperationException("The last message must have a StoredAt value to continue from it.");

        var parameters = this with { SinceDate = last.Value.StoredAt!.Value };
        return parameters;
    }
}
