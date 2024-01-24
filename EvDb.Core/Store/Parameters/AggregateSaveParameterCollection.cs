using System.Collections;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("Count:{Events.Count}")]
public class AggregateSaveParameterCollection<T> : IEnumerable<AggregateSaveParameter/*<T>*/>
{
    private readonly string _domain;
    private readonly string aggregateId;
    private readonly string aggregateType;
    private readonly long baseSeq;
    public AggregateSaveParameterCollection(IEvDbStreamStore aggregate)
    {
        _domain = aggregate.StreamAddress.Domain;
        aggregateId = aggregate.StreamAddress.StreamId;
        aggregateType = aggregate.StreamAddress.Partition;
        baseSeq = aggregate.LastStoredOffset + 1;
        Events = aggregate.Events;
    }
    public IEnumerable<IEvDbEvent> Events { get; }

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
