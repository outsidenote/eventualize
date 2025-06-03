using System.Collections.Immutable;

namespace EvDb.Core;

//[GenerateBuilderPattern]
/// <summary>
/// Event filtering options
/// </summary>
public readonly record struct EvDbMessageFilter
{
    #region Create

    public static EvDbMessageFilter Create(DateTimeOffset since) => new EvDbMessageFilter
    {
        Since = since
    };

    #endregion //  Create

    public EvDbMessageFilter()
    {
    }

    /// <summary>
    /// Defines since when the messages should be fetched.
    /// The result will include messages that were created after this date.
    /// </summary>
    public DateTimeOffset Since { get; init; }

    /// <summary>
    /// Restrict the messages to those that match the specified channels.
    /// Ignore this property if you want to get all messages.
    /// </summary>
    public IImmutableList<EvDbChannelName> Channels { get; init; } = ImmutableList<EvDbChannelName>.Empty;

    /// <summary>
    /// Restrict the messages to those that match the specified message-types.
    /// Ignore this property if you want to get all messages.
    /// </summary>
    public IImmutableList<EvDbMessageTypeName> MessageTypes { get; init; } = ImmutableArray<EvDbMessageTypeName>.Empty;

    #region Operator Overloads

    public static implicit operator EvDbMessageFilter(DateTimeOffset item) => new EvDbMessageFilter
    {
        Since = item
    };

    public static implicit operator EvDbMessageFilter(DateTime item) => new EvDbMessageFilter
    {
        Since = item
    };

    public static implicit operator EvDbMessageFilter(EvDbChannelName item) => new EvDbMessageFilter
    {
        Channels = ImmutableArray.Create(item)
    };

    public static implicit operator EvDbMessageFilter(EvDbMessageTypeName item) => new EvDbMessageFilter
    {
        MessageTypes = ImmutableArray.Create(item)
    };

    #endregion //  Operator Overloads

    #region Add

    public EvDbMessageFilter AddChannel(EvDbChannelName channel)
    {
        return this with { Channels = Channels.Add(channel) };
    }
    public EvDbMessageFilter AddMessageType(EvDbMessageTypeName messageType)
    {
        return this with { MessageTypes = MessageTypes.Add(messageType) };
    }   

    #endregion //  Add
}

