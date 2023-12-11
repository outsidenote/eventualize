using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public class AggregateTests
{
    [Fact]
    public async Task Aggregate_WhenAddingPendingEvent_Succeed()
    {
        var aggregate = TestAggregateConfigs.GetTestAggregate();
        var e = await GetCorrectTestEvent();
        aggregate.AddPendingEvent(e);
        Assert.Single(aggregate.PendingEvents);
        Assert.Equal(aggregate.PendingEvents[0], e);
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithEvents_Succeed()
    {
        List<EventEntity> events = new();
        for (int i = 0; i < 3; i++)
        {
            events.Add(await GetCorrectTestEvent());
        }
        var aggregate = TestAggregateConfigs.GetTestAggregate(events, false);
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        List<EventEntity> events = new();
        for (int i = 0; i < 3; i++)
        {
            events.Add(await GetCorrectTestEvent());
        }
        var aggregate = TestAggregateConfigs.GetTestAggregateFromStore(new TestState(3, 3, 30), events);
        Assert.Empty( aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(6, 6, 60));
    }

    [Fact]
    public void Aggregate_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        var aggregate = TestAggregateConfigs.GetTestAggregate(new TestState(3, 3, 30), new List<EventEntity>());
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

}