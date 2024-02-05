// using System.Diagnostics;

// namespace EvDb.Core;

// [DebuggerDisplay("ID:{Id}, Type:{Type}")]
// public record AggregateParameter(string Id, string Type)
// {
//     public AggregateParameter(EvDbCollectionMeta aggregate)
//         : this(aggregate.StreamAddress.StreamAddress, aggregate.StreamUri.PartitionAddress) { }
//     public EvDbStreamCursor ToSequence(long sequence = 0)
//     {
//         return new EvDbStreamCursor(this, sequence);
//     }
// }
