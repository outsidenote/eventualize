// using System.Diagnostics;

// namespace EvDb.Core;

// [DebuggerDisplay("ID:{Id}, Type:{Type}")]
// public record AggregateParameter(string Id, string Type)
// {
//     public AggregateParameter(EvDbAggregate aggregate)
//         : this(aggregate.StreamId.EntityId, aggregate.StreamUri.EntityType) { }
//     public EvDbStreamCursor ToSequence(long sequence = 0)
//     {
//         return new EvDbStreamCursor(this, sequence);
//     }
// }
