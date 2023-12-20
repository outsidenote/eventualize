namespace Eventualize.Core.Tests;

public static class TestHelper
{
    private static readonly IAsyncEnumerable<EventualizeEvent> _emptyEvents = AsyncEnumerable<EventualizeEvent>.Empty;

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

    public static EventualizeEvent GetCorrectTestEvent()
    {
        return TestEventType.CreateEvent(CorrectEventData, "TestOperation");
    }

    public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents()
    {
        EventualizeAggregate<TestState> aggregate = TestAggregateConfigs.GetTestAggregate();
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(GetCorrectTestEvent());
        return aggregate;

    }

    public static async Task<EventualizeAggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
    {
        var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(_emptyEvents, minEventsBetweenSnapshots);
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(GetCorrectTestEvent());
        return aggregate;

    }
    public static async Task<EventualizeAggregate<TestState>> PrepareAggregateWithEvents()
    {
        List<EventualizeEvent> events = new();
        for (int i = 0; i < 3; i++)
            events.Add(GetCorrectTestEvent());
        return await TestAggregateConfigs.GetTestAggregateAsync(events.ToAsync(), true);
    }
}