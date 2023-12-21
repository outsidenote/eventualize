using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public class AggregateTests
{
    [Fact]
    public void Aggregate_WhenAddingPendingEvent_Succeed()
    {
        var aggregate = TestAggregateConfigs.GetTestAggregate();
        var e = GetCorrectTestEvent();
        aggregate.AddPendingEvent(e);
        Assert.Single(aggregate.PendingEvents);
        Assert.Equal(aggregate.PendingEvents[0], e);
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithEvents_Succeed()
    {
        List<EventualizeEvent> events = new();
        for (int i = 0; i < 3; i++)
        {
            events.Add(GetCorrectTestEvent());
        }
        var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(events.ToAsync(), false);
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        List<EventualizeEvent> events = new();
        for (int i = 0; i < 3; i++)
        {
            events.Add(GetCorrectTestEvent());
        }
        var aggregate = await TestAggregateConfigs.GetTestAggregateFromStore(new TestState(3, 3, 30), events.ToAsync());
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(6, 6, 60));
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        TestState state = new(3, 3, 30);
        var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(state, AsyncEnumerable<EventualizeEvent>.Empty);
        Assert.Empty(aggregate.PendingEvents);
        Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

}