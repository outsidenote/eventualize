namespace Eventualize.Core.Tests;

public static class TestAggregateConfigs
{
    public static EventualizeAggregate<TestState> GetTestAggregate()
    {
        return TestAggregateFactoryConfigs.GetAggregateFactory().Create(Guid.NewGuid().ToString());
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<EventualizeStoredEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var result = await TestAggregateFactoryConfigs.GetAggregateFactory().CreateAsync(id, events);
        return result;
    }

    public static EventualizeAggregate<TestState> GetTestAggregate(IEnumerable<EventualizeEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var aggregate = TestAggregateFactoryConfigs.GetAggregateFactory().Create(id);
        foreach (var e in events)
        {
            aggregate.AddPendingEvent(e);
        }
        return aggregate;
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<EventualizeStoredEvent>? events, int? minEventsBetweenSnapshots)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(minEventsBetweenSnapshots ?? 3);
        if (events == null)
            return aggregateFactory.Create(Guid.NewGuid().ToString());
        else
            return await aggregateFactory.CreateAsync(Guid.NewGuid().ToString(), events);
    }

    public static EventualizeAggregate<TestState> GetTestAggregate(IEnumerable<EventualizeEvent>? events, int? minEventsBetweenSnapshots)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(minEventsBetweenSnapshots ?? 3);
        var aggregate = aggregateFactory.Create(Guid.NewGuid().ToString());
        if (events != null)
            foreach (var e in events)
            {
                aggregate.AddPendingEvent(e);
            }
        return aggregate;
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(TestState snapshot, IAsyncEnumerable<EventualizeStoredEvent> events)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
        var snap = EventualizeStoredSnapshotData<TestState>.Create(snapshot);
        return await aggregateFactory.CreateAsync(
            Guid.NewGuid().ToString(),
            events,
            snap
        );
    }

    public static IAsyncEnumerable<EventualizeStoredEvent> GetStoredEvents(uint numEvents)
    {
        List<EventualizeStoredEvent> events = new();
        for (int offset = 0; offset < 3; offset++)
        {
            events.Add(TestHelper.GetCorrectTestEvent(offset));
        }
        return events.ToAsync();
    }

    public static IEnumerable<EventualizeEvent> GetPendingEvents(uint numEvents)
    {
        IEnumerable<EventualizeEvent> events = [];
        for (int offset = 0; offset < 3; offset++)
        {
            events = events.Concat([TestHelper.GetCorrectTestEvent()]);
        }
        return events;
    }
}