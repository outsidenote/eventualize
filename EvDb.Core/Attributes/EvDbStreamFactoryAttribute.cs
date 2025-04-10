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
    public EvDbStreamFactoryAttribute(string domain, string partition) : base(domain, partition)
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
    /// <param name="domain">
    /// The `domain` and `partition` are the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </param>
    /// <param name="partition">
    /// The `domain` and `partition` are the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </param>
    public EvDbStreamFactoryAttribute(string domain, string partition)
    {
        Domain = domain;
        Partition = partition;
    }

    /// <summary>
    /// The `domain` and `partition` are the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </summary>
    public string Domain { get; }

    /// <summary>
    /// The `domain` and `partition` are the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </summary>
    public string Partition { get; }

    /// <summary>
    /// Set the name of the stream object
    /// </summary>
    public string Name { get; init; } = string.Empty;

}