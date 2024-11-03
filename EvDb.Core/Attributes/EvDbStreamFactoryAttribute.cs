#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <typeparam name="TTopicProducer">The type of the public event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbStreamFactoryAttribute<TEventType, TTopicProducer> : EvDbStreamFactoryAttribute<TEventType>
    where TEventType : IEvDbEventTypes
    where TTopicProducer : IEvDbTopicProducer
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
    public EvDbStreamFactoryAttribute(string domain, string partition)
    {
        Domain = domain;
        Partition = partition;
    }

    public string Domain { get; }
    public string Partition { get; }
}