using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core.Tests;

public static class TestHelper
{
    private static readonly IAsyncEnumerable<EventualizeStoredEvent> _emptyEvents = AsyncEnumerable<EventualizeStoredEvent>.Empty;

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

    public static readonly EventualizeEventType TestEventType = new EventualizeEventType("testType", typeof(TestEventDataType));
    public static readonly TestEventDataType CorrectEventData = new("test", 10);

    public static EventualizeStreamAddress GetStreamAddress()
    {
        return new(TestAggregateFactoryConfigs.GetStreamBaseAddress(), "testStreamId");
    }

    public static EventualizeEvent GetCorrectTestEvent()
    {
        return TestEventType.CreateEvent(CorrectEventData, "TestOperation");
    }

    public static EventualizeStoredEvent GetCorrectTestEvent(long offset)
    {
        return new EventualizeStoredEvent(TestEventType.CreateEvent(CorrectEventData, "TestOperation"), GetStreamAddress(), offset);
    }

    public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents()
    {
        EventualizeAggregate<TestState> aggregate = TestAggregateConfigs.GetTestAggregate();
        return PrepareAggregateWithPendingEvents(aggregate);

    }

    public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents(EventualizeAggregate<TestState> aggregate)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
        var newLastStoreOffset = aggregate.LastStoredOffset + aggregate.PendingEvents.Count;
        var newAggregate = aggregateFactory.Create(aggregate.StreamAddress.StreamId, aggregate.State, newLastStoreOffset);
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
        List<EventualizeEvent> events = (List<EventualizeEvent>)TestAggregateConfigs.GetStoredEvents(3);
        return TestAggregateConfigs.GetTestAggregate(events);
    }
}