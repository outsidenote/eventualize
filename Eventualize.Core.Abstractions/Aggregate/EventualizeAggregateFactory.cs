// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset

using Eventualize.Core;

namespace Eventualize.Core;

public class EventualizeAggregateFactory<T> where T : notnull, new()
{
    #region Members
    public readonly EventualizeFoldingLogic<T> FoldingLogic;

    public readonly string AggregateType;
    public readonly EventualizeStreamBaseUri StreamBaseUri;
    public readonly int MinEventsBetweenSnapshots;

    #endregion // Members

    public EventualizeAggregateFactory(
                string aggregateType,
                EventualizeStreamBaseUri streamBaseAddress, 
                EventualizeFoldingLogic<T> foldingLogic,
                int minEventsBetweenSnapshots = 0)
    {
        AggregateType = aggregateType;
        StreamBaseUri = streamBaseAddress;
        FoldingLogic = foldingLogic;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }

    /// <summary>
    /// Creates the aggregate instance with specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public EventualizeAggregate<T> Create(string id)
    {
        var result = new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseUri, id),  FoldingLogic, MinEventsBetweenSnapshots);
        return result;
    }

    /// <summary>
    /// Creates the aggregate instance with specified identifier and initial events, snapshot.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="storedEvents">The stored events.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns></returns>
    public async Task<EventualizeAggregate<T>> CreateAsync(
            string id,
            IAsyncEnumerable<IEventualizeStoredEvent> storedEvents,
            EventualizeStoredSnapshot<T>? snapshot = null)
    {
        var snp = snapshot ?? EventualizeStoredSnapshot<T>.Create();
        long offset = snp.Cursor.Offset;
        T state = snp.State;
        await foreach (var e in storedEvents)
        {
            state = FoldingLogic.FoldEvent(state, e);
            offset = e.StreamCursor.Offset;
        }
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseUri, id),  FoldingLogic, MinEventsBetweenSnapshots, state, offset);
    }

    public EventualizeAggregate<T> Create(string id, T snapshot, long snapshotOffset)
    {
        EventualizeStreamUri streamUri = new EventualizeStreamUri(StreamBaseUri, id);
        EventualizeAggregate<T> result =  new (
                                               AggregateType,
                                               streamUri,
                                               FoldingLogic,
                                               MinEventsBetweenSnapshots,
                                               snapshot,
                                               snapshotOffset);
        return result;
    }
}

