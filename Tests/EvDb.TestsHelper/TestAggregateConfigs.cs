namespace EvDb.Core.Tests;

public static class TestAggregateConfigs
{
    public static EvDbAggregate<TestState> GetTestAggregate(bool useFoldingLogic2 = false)
    {
        return TestAggregateFactoryConfigs.GetAggregateFactory(useFoldingLogic2).Create(Guid.NewGuid().ToString());
    }

    public static async Task<EvDbAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<IEvDbStoredEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var result = await TestAggregateFactoryConfigs.GetAggregateFactory().CreateAsync(id, events);
        return result;
    }

    public static EvDbAggregate<TestState> GetTestAggregate(IEnumerable<IEvDbEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var aggregate = TestAggregateFactoryConfigs.GetAggregateFactory().Create(id);
        foreach (var e in events)
        {
            aggregate.AddPendingEvent(e);
        }
        return aggregate;
    }

    public static async Task<EvDbAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<IEvDbStoredEvent>? events, int? minEventsBetweenSnapshots)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory(minEventsBetweenSnapshots ?? 3);
        if (events == null)
            return aggregateFactory.Create(Guid.NewGuid().ToString());
        else
            return await aggregateFactory.CreateAsync(Guid.NewGuid().ToString(), events);
    }

    public static EvDbAggregate<TestState> GetTestAggregate(IEnumerable<IEvDbEvent>? events, int? minEventsBetweenSnapshots)
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

    public static async Task<EvDbAggregate<TestState>> GetTestAggregateAsync(TestState snapshot, IAsyncEnumerable<IEvDbStoredEvent> events)
    {
        var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
        var snap = EvDbStoredSnapshot.Create<TestState>(snapshot);
        return await aggregateFactory.CreateAsync(
            Guid.NewGuid().ToString(),
            events,
            snap
        );
    }

    public static IAsyncEnumerable<IEvDbStoredEvent> GetStoredEvents(uint numEvents)
    {
        List<IEvDbStoredEvent> events = new();
        for (int offset = 0; offset < 3; offset++)
        {
            events.Add(TestHelper.GetCorrectTestEvent(offset));
        }
        return events.ToAsync();
    }

    public static IEnumerable<IEvDbEvent> GetPendingEvents(uint numEvents)
    {
        IEnumerable<IEvDbEvent> events = [];
        for (int offset = 0; offset < 3; offset++)
        {
            events = events.Concat([TestHelper.GetCorrectTestEvent()]);
        }
        return events;
    }
}