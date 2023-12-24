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
        _domain = aggregate.StreamAddress.Domain;
        aggregateId = aggregate.StreamAddress.StreamId;
        aggregateType = aggregate.StreamAddress.StreamType;
        baseSeq = aggregate.LastStoredOffset + 1;
        Events = aggregate.PendingEvents;
    }
    public IImmutableList<EventualizeEvent> Events { get; }

    public IEnumerator<AggregateSaveParameter> GetEnumerator()
    {
        int i = 0;
        foreach (var item in Events)
        {
            var e = new AggregateSaveParameter/*<T>*/(
                            aggregateId,
                            aggregateType,
                            item.EventType,
                            baseSeq + i,
                            item.JsonData,
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
