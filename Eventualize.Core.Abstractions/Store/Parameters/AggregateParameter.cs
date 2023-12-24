// using System.Diagnostics;

// namespace Eventualize.Core;

// [DebuggerDisplay("ID:{Id}, Type:{Type}")]
// public record AggregateParameter(string Id, string Type)
// {
//     public AggregateParameter(EventualizeAggregate aggregate)
//         : this(aggregate.StreamUri.StreamId, aggregate.StreamUri.StreamType) { }
//     public EventualizeStreamCursor ToSequence(long sequence = 0)
//     {
//         return new EventualizeStreamCursor(this, sequence);
//     }
// }
