namespace EvDb.Core;

/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbStreamFactoryAttribute<TEventType> : Attribute
    where TEventType : IEvDbEventAdder
{
    public EvDbStreamFactoryAttribute(string domain, string partition)
    {
        Domain = domain;
        Partition = partition;
    }

    public string Domain { get; }
    public string Partition { get; }
}