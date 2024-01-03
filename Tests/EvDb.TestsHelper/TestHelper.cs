namespace EvDb.Core.Tests;

public static class TestHelper
{
    private static readonly IAsyncEnumerable<IEvDbStoredEvent> _emptyEvents = AsyncEnumerable<IEvDbStoredEvent>.Empty;

    public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> self)
    {
        foreach (var item in self)
        {
            await Task.Yield();
            yield return item;
        }
    }

#pragma warning disable S5034 // "ValueTask" should be consumed correctly
    public static async Task<ICollection<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> self)
    {
        var list = new List<T>();
        await foreach (var item in self)
        {
            list.Add(item);
        }
        return list;
    }
#pragma warning restore S5034 // "ValueTask" should be consumed correctly

    public static readonly string TestEventType = "testType";
    public static readonly TestEventDataType CorrectEventData = new("test", 10);

    public readonly static EvDbStreamAddress StreamId = new(
                                TestAggregateFactoryConfigs.GetStreamType,
                                "testStreamId");

    public readonly static IEvDbEventFactory<TestEventDataType> TestEventFactory =
        new EvDbEventFactory<TestEventDataType>(TestEventType, "TestOperation");

    public static IEvDbEvent GetCorrectTestEvent()
    {
        return TestEventFactory.Create(CorrectEventData);
    }

    public static IEvDbStoredEvent GetCorrectTestEvent(long offset)
    {
        var cursor = new EvDbStreamCursor(StreamId, offset);
        return EvDbStoredEventFactory.Create(TestEventType,
            DateTime.UtcNow,
            "TestOperation",
            CorrectEventData,
            DateTime.UtcNow,
            cursor);
    }

    public static EvDbAggregate<TestState> PrepareAggregateWithPendingEvents(bool useFoldingLogic2 = false)
    {
        EvDbAggregate<TestState> aggregate = TestAggregateConfigs.GetTestAggregate(useFoldingLogic2);
        return PrepareAggregateWithPendingEvents(aggregate);

    }

    public static EvDbAggregate<TestState> PrepareAggregateWithPendingEvents(EvDbAggregate<TestState> aggregate, bool useFoldingLogic2 = false)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(useFoldingLogic2);
        var newLastStoreOffset = aggregate.LastStoredOffset + aggregate.PendingEvents.Count;
        var newAggregate = aggregateFactory.Create(aggregate.StreamId.StreamId, aggregate.State, newLastStoreOffset);
        var events = TestAggregateConfigs.GetPendingEvents(3);
        foreach (var e in events)
        {
            newAggregate.AddPendingEvent(e);
        }
        return newAggregate;

    }

    public static async Task<EvDbAggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
    {
        var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(_emptyEvents, minEventsBetweenSnapshots);
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(GetCorrectTestEvent());
        return aggregate;

    }
    public static EvDbAggregate<TestState> PrepareAggregateWithEvents()
    {
        List<IEvDbEvent> events = (List<IEvDbEvent>)TestAggregateConfigs.GetStoredEvents(3);
        return TestAggregateConfigs.GetTestAggregate(events);
    }
}