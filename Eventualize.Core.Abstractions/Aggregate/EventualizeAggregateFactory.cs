// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotSequenceId 

namespace Eventualize.Core;

public class EventualizeAggregateFactory
{
    internal static EventualizeAggregate<T> Create<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots)
                where T : notnull, new()
    {
        EventualizeAggregate<T> aggregate = new(aggregateType, id, minEventsBetweenSnapshots);
        return aggregate;
    }

    public static async Task<EventualizeAggregate<T>> CreateAsync<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            IAsyncEnumerable<EventualizeEvent> events)
                where T : notnull, new()
    {
        var result =
                        await CreateAsync(
                                aggregateType,
                                id,
                                minEventsBetweenSnapshots,
                                events, new T());
        return result;
    }

    public static async Task<EventualizeAggregate<T>> CreateAsync<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            IAsyncEnumerable<EventualizeEvent> events,
            T snapshot,
            long snapshotSequenceId = -1)
                where T : notnull, new()
    {
        var (state, count) = await aggregateType.FoldEventsAsync(snapshot, events);
        long lastStoredSequenceId = snapshotSequenceId + count;
        EventualizeAggregate<T> aggregate = new(
                                        state,
                                        aggregateType,
                                        id,
                                        minEventsBetweenSnapshots,
                                        lastStoredSequenceId);
        return aggregate;
    }

    public static EventualizeAggregate<T> Create<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            T snapshot,
            long snapshotSequenceId)
                where T : notnull, new()
    {
        EventualizeAggregate<T> aggregate = new(
                                                snapshot,
                                                aggregateType,
                                                id,
                                                minEventsBetweenSnapshots,
                                                snapshotSequenceId);
        return aggregate;
    }
}

