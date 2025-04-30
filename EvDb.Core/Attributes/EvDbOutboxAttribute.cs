#pragma warning disable CS1712 // Type parameter has no matching typeparam tag in the XML comment (but other type parameters do)
// Ignore Spelling: OutboxProducer

#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// OutboxProducer pattern provider.
/// </summary>
/// <typeparam name="TStreamFactory">
/// Reference to a stream factory.
/// </typeparam>
/// <seealso cref="System.Attribute" />
/// <remarks>
/// The Outbox Pattern ensures reliable message delivery 
/// in distributed systems by storing events or messages 
/// in a local "outbox" table within the same database transaction 
/// as the business operation. These messages are then asynchronously 
/// processed and sent to external systems, ensuring consistency 
/// between the database and message broker.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbOutboxAttribute<TStreamFactory> : Attribute
#pragma warning restore CS1712 // Type parameter has no matching typeparam tag in the XML comment (but other type parameters do)
    where TStreamFactory : IEvDbStreamConfig
{
    public EvDbOutboxAttribute(string? defaultShardName = null)
    {
        DefaultShardName = defaultShardName;
    }
    /// <summary>
    /// Default shard name of the outbox storage unit (table/collection).
    /// </summary>
    public string? DefaultShardName { get; }
}

/// <summary>
/// OutboxProducer pattern provider.
/// </summary>
/// <typeparam name="TStreamFactory">
/// Reference to a stream factory.
/// </typeparam>
/// <typeparam name="TShards">
/// Reference to the sharding options.
/// </typeparam>
/// <seealso cref="System.Attribute" />
/// <remarks>
/// The Outbox Pattern ensures reliable message delivery 
/// in distributed systems by storing events or messages 
/// in a local "outbox" table within the same database transaction 
/// as the business operation. These messages are then asynchronously 
/// processed and sent to external systems, ensuring consistency 
/// between the database and message broker.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbOutboxAttribute<TStreamFactory, TShards> : Attribute
#pragma warning restore CS1712 // Type parameter has no matching typeparam tag in the XML comment (but other type parameters do)
    where TStreamFactory : IEvDbStreamConfig
{
    public EvDbOutboxAttribute(string? defaultShardName = null)
    {
        DefaultShardName = defaultShardName;
    }
    /// <summary>
    /// Default shard name of the outbox storage unit (table/collection).
    /// </summary>
    public string? DefaultShardName { get; }
}
