using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("ID:{Id}, Type:{Type}")]
public record AggregateParameter(string Id, string Type)
{
    public AggregateSequenceParameter ToSequence(long sequence = 0)
    {
        return new AggregateSequenceParameter(this, sequence);
    }
}
