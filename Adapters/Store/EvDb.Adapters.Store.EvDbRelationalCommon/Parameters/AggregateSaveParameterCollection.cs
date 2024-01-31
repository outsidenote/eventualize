using System.Collections;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("Count:{Events.Count}")]
public class AggregateSaveParameterCollection<T> : IEnumerable<StoreEventsParameter>
{
    private readonly string _domain;
    private readonly string aggregateId;
    private readonly string aggregateType;
    private readonly long baseSeq;
    public AggregateSaveParameterCollection(IEvDbStreamStore store)
    {
        IEvDbStreamStoreData self = (IEvDbStreamStoreData)store;
        _domain = store.StreamAddress.Domain;
        aggregateId = store.StreamAddress.StreamId;
        aggregateType = store.StreamAddress.Partition;
        baseSeq = store.StoreOffset + 1;
        Events = self.Events;
    }
    public IEnumerable<EvDbEvent> Events { get; }

    public IEnumerator<StoreEventsParameter> GetEnumerator()
    {
        foreach (EvDbEvent e in Events)
        {
            yield return e;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
