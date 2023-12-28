namespace Eventualize.Core.Tests;

public static class TestAggregateConfigs
{
    public static EventualizeAggregate<TestState> GetTestAggregate(bool useFoldingLogic2 = false)
    {
        return TestAggregateFactoryConfigs.GetAggregateFactory(useFoldingLogic2).Create(Guid.NewGuid().ToString());
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<IEventualizeStoredEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var result = await TestAggregateFactoryConfigs.GetAggregateFactory().CreateAsync(id, events);
        return result;
    }

    public static EventualizeAggregate<TestState> GetTestAggregate(IEnumerable<IEventualizeEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var aggregate = TestAggregateFactoryConfigs.GetAggregateFactory().Create(id);
        foreach (var e in events)
        {
            aggregate.AddPendingEvent(e);
        }
        return aggregate;
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<IEventualizeStoredEvent>? events, int? minEventsBetweenSnapshots)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(minEventsBetweenSnapshots ?? 3);
        if (events == null)
            return aggregateFactory.Create(Guid.NewGuid().ToString());
        else
            return await aggregateFactory.CreateAsync(Guid.NewGuid().ToString(), events);
    }

    public static EventualizeAggregate<TestState> GetTestAggregate(IEnumerable<IEventualizeEvent>? events, int? minEventsBetweenSnapshots)
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

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(TestState snapshot, IAsyncEnumerable<IEventualizeStoredEvent> events)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
        var snap = EventualizeStoredSnapshot.Create<TestState>(snapshot);
        return await aggregateFactory.CreateAsync(
            Guid.NewGuid().ToString(),
            events,
            snap
        );
    }

    public static IAsyncEnumerable<IEventualizeStoredEvent> GetStoredEvents(uint numEvents)
    {
        List<IEventualizeStoredEvent> events = new();
        for (int offset = 0; offset < 3; offset++)
        {
            events.Add(TestHelper.GetCorrectTestEvent(offset));
        }
        return events.ToAsync();
    }

    public static IEnumerable<IEventualizeEvent> GetPendingEvents(uint numEvents)
    {
        IEnumerable<IEventualizeEvent> events = [];
        for (int offset = 0; offset < 3; offset++)
        {
            events = events.Concat([TestHelper.GetCorrectTestEvent()]);
        }
        return events;
    }
}