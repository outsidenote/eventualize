using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Eventualize.Core;

[DebuggerDisplay("Count:{Events.Count}")]
public class AggregateSaveParameterCollection<T> : IEnumerable<AggregateSaveParameter/*<T>*/>
    where T : notnull, new()
{
    private readonly string _domain;
    private readonly string aggregateId;
    private readonly string aggregateType;
    private readonly long baseSeq;
    public AggregateSaveParameterCollection(EventualizeAggregate<T> aggregate)
    {
        _domain = aggregate.StreamUri.Domain;
        aggregateId = aggregate.StreamUri.StreamId;
        aggregateType = aggregate.StreamUri.StreamType;
        baseSeq = aggregate.LastStoredOffset + 1;
        Events = aggregate.PendingEvents;
    }
    public IImmutableList<IEventualizeEvent> Events { get; }

    public IEnumerator<AggregateSaveParameter> GetEnumerator()
    {
        int i = 0;
        foreach (IEventualizeEvent item in Events)
        {
            var e = new AggregateSaveParameter/*<T>*/(
                            aggregateId,
                            aggregateType,
                            item.EventType,
                            baseSeq + i,
                            item.GetData(),
                            item.CapturedBy,
                            item.CapturedAt,
                            _domain
                        );
            yield return e;
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
