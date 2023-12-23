namespace Eventualize.Core.Tests;

public static class TestAggregateConfigs
{
    public static EventualizeAggregate<TestState> GetTestAggregate()
    {
        return TestAggregateFactoryConfigs.TestAggregateFactory.Create(Guid.NewGuid().ToString());
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<EventualizeStoredEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var result = await TestAggregateFactoryConfigs.TestAggregateFactory.CreateAsync(id, events);
        return result;
    }

    public static EventualizeAggregate<TestState> GetTestAggregate(List<EventualizeEvent> events)
    {
        var id = Guid.NewGuid().ToString();
        var aggregate = TestAggregateFactoryConfigs.TestAggregateFactory.Create(id);
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

    public static EventualizeAggregate<TestState> GetTestAggregate(List<EventualizeEvent>? events, int? minEventsBetweenSnapshots)
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
        return await aggregateFactory.CreateAsync(
            Guid.NewGuid().ToString(),
            events,
            snapshot
        );
    }

    public static IAsyncEnumerable<EventualizeStoredEvent> GetStoredEvents(uint numEvents)
    {
        List<EventualizeStoredEvent> events = new();
        for (int sequenceId = 0; sequenceId < 3; sequenceId++)
        {
            events.Add(TestHelper.GetCorrectTestEvent(sequenceId));
        }
        return events.ToAsync();
    }

    public static IEnumerable<EventualizeEvent> GetPendingEvents(uint numEvents)
    {
        IEnumerable<EventualizeEvent> events = [];
        for (int sequenceId = 0; sequenceId < 3; sequenceId++)
        {
            events.Append(TestHelper.GetCorrectTestEvent());
        }
        return events;
    }
}