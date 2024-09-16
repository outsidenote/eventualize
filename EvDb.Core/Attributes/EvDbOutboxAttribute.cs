// Ignore Spelling: OutboxHandler

#pragma warning disable S2326 // Unused type parameters should be removed

namespace EvDb.Core;

/// <summary>
/// OutboxHandler pattern provider.
/// </summary>
/// <typeparam name="TStreamFactory">
/// Reference to a stream factory.
/// </typeparam>
/// <seealso cref="System.Attribute" />
/// <remarks>
/// The OutboxHandler Pattern ensures reliable message delivery 
/// in distributed systems by storing events or messages 
/// in a local "outbox" table within the same database transaction 
/// as the business operation. These messages are then asynchronously 
/// processed and sent to external systems, ensuring consistency 
/// between the database and message broker.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbOutboxAttribute<TStreamFactory> : Attribute
    where TStreamFactory : IEvDbStreamConfig
{
}
