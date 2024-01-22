// using System.Diagnostics;

// namespace EvDb.Core;

// [DebuggerDisplay("ID:{Id}, Type:{Type}")]
// public record AggregateParameter(string Id, string Type)
// {
//     public AggregateParameter(EvDbCollectionMeta aggregate)
//         : this(aggregate.StreamId.StreamId, aggregate.StreamUri.Partition) { }
//     public EvDbStreamCursor ToSequence(long sequence = 0)
//     {
//         return new EvDbStreamCursor(this, sequence);
//     }
// }
