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
    public EvDbStreamFactoryAttribute(string rootAddress) : base(rootAddress)
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
    /// <param name="rootAddress">
    /// The `rootAddress` is the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </param>
    public EvDbStreamFactoryAttribute(string rootAddress)
    {
        RootAddress = rootAddress;
    }

    /// <summary>
    /// The `rootAddress` is the static part of the stream address (uniqueness).
    /// Along with the stream id that is the dynamic part of the address
    /// </summary>
    public string RootAddress { get; }

    /// <summary>
    /// Set the name of the stream object
    /// </summary>
    public string Name { get; init; } = string.Empty;

}