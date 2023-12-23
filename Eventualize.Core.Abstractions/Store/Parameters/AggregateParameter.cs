using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}")]
public record AggregateParameter(string Id, string Type)
{
    public AggregateParameter(EventualizeAggregate aggregate)
        : this(aggregate.StreamAddress.StreamId, aggregate.StreamAddress.StreamType) { }
    public AggregateSequenceParameter ToSequence(long sequence = 0)
    {
        return new AggregateSequenceParameter(this, sequence);
    }
}
