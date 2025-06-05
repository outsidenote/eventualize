// Ignore Spelling: Occ

using System.Data;
using System.Threading.Channels;

namespace EvDb.Core.Adapters.Internals;

public readonly record struct EvDbGetMessagesParameters
{
    private const int BATCH_SIZE = 300;

    public EvDbGetMessagesParameters(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions options)
    {
        Channels = filter.Channels?.Any() == true
                        ? filter.Channels.Select(m => (string)m).ToArray()
                        : null;
        _channels = new HashSet<string>(Channels ?? []);
        MessageTypes = filter.MessageTypes?.Any() == true
                        ? filter.MessageTypes.Select(m => (string)m).ToArray()
                        : null;
        _messageTypes = new HashSet<string>(MessageTypes ?? []);
        SinceDate = filter.Since;
    }

    public int BatchSize { get; } = BATCH_SIZE; // Default batch size for fetching messages

    private readonly HashSet<string> _channels;
    public bool IncludeChannel(EvDbChannelName channel) => _channels.Count == 0 || _channels.Contains(channel.Value, StringComparer.OrdinalIgnoreCase);
                            

    public string[]? Channels { get; }

    private readonly HashSet<string> _messageTypes;
    public bool IncludeMessageType(EvDbMessageTypeName messageType) => _messageTypes.Count == 0 || _messageTypes.Contains(messageType.Value, StringComparer.OrdinalIgnoreCase);
    public string[]? MessageTypes { get; }

    public DateTimeOffset SinceDate { get; init; }

    public EvDbGetMessagesParameters ContinueFrom(EvDbMessage? last)
    {
        if (last == null)
            return this;

        if(!last.Value.StoredAt.HasValue)
            throw new InvalidOperationException("The last message must have a StoredAt value to continue from it.");

        var parameters = this with { SinceDate = last.Value.StoredAt!.Value};
        return parameters;
    }
}
