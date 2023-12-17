namespace Eventualize.Core.Tests;

public static class TestAggregateConfigs
{
    public static EventualizeAggregate<TestState> GetTestAggregate()
    {
        var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
        return aggregateType.CreateAggregate(Guid.NewGuid().ToString());
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<EventualizeEvent> events, bool isPendingEvents)
    {
        var id = Guid.NewGuid().ToString();
        var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
        if (!isPendingEvents)
        {
            var result = await aggregateType.CreateAggregateAsync(id, events);
            return result;
        }
        else
        {
            var aggregate = aggregateType.CreateAggregate(id);
            await foreach (var e in events)
            {
                aggregate.AddPendingEvent(e);
            }
            return aggregate;
        }

    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(IAsyncEnumerable<EventualizeEvent>? events, int? minEventsBetweenSnapshots)
    {
        var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogicAndMinEvents(minEventsBetweenSnapshots);
        if (events == null)
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString());
        else
            return await aggregateType.CreateAggregateAsync(Guid.NewGuid().ToString(), events);
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateAsync(TestState snapshot, IAsyncEnumerable<EventualizeEvent> events)
    {
        var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
        var aggregate = await aggregateType.CreateAggregateAsync(
                            Guid.NewGuid().ToString(),
                            new List<EventualizeEvent>().ToAsync(),
                            snapshot,
                            (long)0);
        await foreach (var e in events)
        {
            aggregate.AddPendingEvent(e);
        }
        return aggregate;
    }

    public static async Task<EventualizeAggregate<TestState>> GetTestAggregateFromStore(TestState snapshot, IAsyncEnumerable<EventualizeEvent> events)
    {
        var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
        return await aggregateType.CreateAggregateAsync(Guid.NewGuid().ToString(), events, snapshot, (long)0);
    }
}