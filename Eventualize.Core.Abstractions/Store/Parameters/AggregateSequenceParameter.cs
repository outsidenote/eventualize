using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}, Seq:{Sequence}")]
public record AggregateSequenceParameter(string Id, string Type, long Sequence = 0):  AggregateParameter(Id, Type)
{
    public AggregateSequenceParameter(AggregateParameter copy, long sequence = 0)
        : this (copy.Id, copy.Type, sequence)
    { 
    }
    public AggregateSequenceParameter(EventualizeAggregate aggregate)
        : this (aggregate.StreamAddress.StreamId, aggregate.StreamAddress.StreamType, aggregate.LastStoredOffset + 1)
    { 
    }
}
