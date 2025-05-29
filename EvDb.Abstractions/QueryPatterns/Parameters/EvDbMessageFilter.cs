using System.Collections.Immutable;

namespace EvDb.Core;

//[GenerateBuilderPattern]
/// <summary>
/// Event filtering options
/// </summary>
public readonly record struct EvDbMessageFilter
{
    public EvDbMessageFilter()
    {
    }

    public EvDbShardName Shard { get; init; } = EvDbShardName.Default;

    /// <summary>
    /// Defines since when the messages should be fetched.
    /// The result will include messages that were created after this date.
    /// </summary>
    public DateTimeOffset Since { get; init; }

    /// <summary>
    /// Restrict the messages to those that match the specified text filter.
    /// Ignore this property if you want to get all messages.
    /// </summary>
    public IImmutableList<EvDbChannelName> Channels { get; init; }

}

