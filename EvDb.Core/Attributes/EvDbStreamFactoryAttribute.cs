#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <typeparam name="TOutboxProducer">The type of the public event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbStreamFactoryAttribute<TEventType, TOutboxProducer> : EvDbStreamFactoryAttribute<TEventType>
    where TEventType : IEvDbEventTypes
    where TOutboxProducer : IEvDbOutboxProducer
{
    public EvDbStreamFactoryAttribute(string streamType) : base(streamType)
    {
    }

}

/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbStreamFactoryAttribute<TEventType> : Attribute
    where TEventType : IEvDbEventTypes
{
    /// <summary>
    /// Generate a EventualizeDB client factory
    /// </summary>
    /// <param name="streamType">
    /// The `streamType` is the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </param>
    public EvDbStreamFactoryAttribute(string streamType)
    {
        StreamType = streamType;
    }

    /// <summary>
    /// The `streamType` is the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </summary>
    public string StreamType { get; }

    /// <summary>
    /// Set the name of the stream object
    /// </summary>
    public string Name { get; init; } = string.Empty;

}