using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("Count:{Events.Count}")]
public class AggregateSaveParameterCollection<T> : IEnumerable<AggregateSaveParameter/*<T>*/>
    where T : notnull, new()
{
    private readonly string _domain;
    private readonly string aggregateId;
    private readonly string aggregateType;
    private readonly long baseSeq;
    public AggregateSaveParameterCollection(EvDbAggregate<T> aggregate)
    {
        _domain = aggregate.StreamId.Domain;
        aggregateId = aggregate.StreamId.EntityId;
        aggregateType = aggregate.StreamId.EntityType;
        baseSeq = aggregate.LastStoredOffset + 1;
        Events = aggregate.PendingEvents;
    }
    public IImmutableList<IEvDbEvent> Events { get; }

    public IEnumerator<AggregateSaveParameter> GetEnumerator()
    {
        int i = 0;
        foreach (IEvDbEvent e in Events)
        {
            EvDbEvent entity = (EvDbEvent)e;
            var item = new AggregateSaveParameter(
                            aggregateId,
                            aggregateType,
                            entity.EventType,
                            baseSeq + i,
                            entity.Data,
                            entity.CapturedBy,
                            entity.CapturedAt,
                            _domain
                        );
            yield return item;
            i++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
