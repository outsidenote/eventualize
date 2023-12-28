using Eventualize.Core.Abstractions;

namespace Eventualize.Core.Tests;

public static class TestHelper
{
    private static readonly IAsyncEnumerable<IEventualizeStoredEvent> _emptyEvents = AsyncEnumerable<IEventualizeStoredEvent>.Empty;

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

    public readonly static EventualizeStreamUri StreamUri = new(
                                TestAggregateFactoryConfigs.GetStreamBaseUri,
                                "testStreamId");

    public static IEventualizeEvent GetCorrectTestEvent()
    {
        return EventualizeEventFactory.Create(
            TestEventType,
            DateTime.UtcNow,
            "TestOperation",
            CorrectEventData);
    }

    public static IEventualizeStoredEvent GetCorrectTestEvent(long offset)
    {
        var cursor = new EventualizeStreamCursor(StreamUri, offset);
        return EventualizeStoredEventFactory.Create(TestEventType,
            DateTime.UtcNow,
            "TestOperation",
            CorrectEventData,
            DateTime.UtcNow,
            cursor);
    }

    public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents(bool useFoldingLogic2 = false)
    {
        EventualizeAggregate<TestState> aggregate = TestAggregateConfigs.GetTestAggregate(useFoldingLogic2);
        return PrepareAggregateWithPendingEvents(aggregate);

    }

    public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents(EventualizeAggregate<TestState> aggregate, bool useFoldingLogic2 = false)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(useFoldingLogic2);
        var newLastStoreOffset = aggregate.LastStoredOffset + aggregate.PendingEvents.Count;
        var newAggregate = aggregateFactory.Create(aggregate.StreamUri.StreamId, aggregate.State, newLastStoreOffset);
        var events = TestAggregateConfigs.GetPendingEvents(3);
        foreach (var e in events)
        {
            newAggregate.AddPendingEvent(e);
        }
        return newAggregate;

    }

    public static async Task<EventualizeAggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
    {
        var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(_emptyEvents, minEventsBetweenSnapshots);
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(GetCorrectTestEvent());
        return aggregate;

    }
    public static EventualizeAggregate<TestState> PrepareAggregateWithEvents()
    {
        List<IEventualizeEvent> events = (List<IEventualizeEvent>)TestAggregateConfigs.GetStoredEvents(3);
        return TestAggregateConfigs.GetTestAggregate(events);
    }
}