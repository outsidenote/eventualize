namespace EvDb.Core.Tests;

using static EvDb.Core.Tests.TestHelper;

public class AggregateTests
{
    [Fact]
    public void Aggregate_WhenAddingPendingEvent_Succeed()
    {
        throw new NotImplementedException();

        //var aggregate = TestAggregateConfigs.GetTestAggregate();
        //var e = GetCorrectTestEvent();
        //aggregate.AddEvent(e);
        //Assert.Single(aggregate.PendingEvents);
        //Assert.Equal(aggregate.PendingEvents[0], e);
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithEvents_Succeed()
    {
        throw new NotImplementedException();

        //IAsyncEnumerable<IEvDbStoredEvent> events = TestAggregateConfigs.GetStoredEvents(3);
        //var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(events);
        //Assert.Empty(aggregate.PendingEvents);
        //Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        throw new NotImplementedException();

        //IAsyncEnumerable<IEvDbStoredEvent> events = TestAggregateConfigs.GetStoredEvents(3);
        //var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(new TestState(3, 3, 30), events);
        //Assert.Empty(aggregate.PendingEvents);
        //Assert.Equal(aggregate.State, new TestState(6, 6, 60));
    }

    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        throw new NotImplementedException();

        //TestState state = new(3, 3, 30);
        //var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(state, AsyncEnumerable<IEvDbStoredEvent>.Empty);
        //Assert.Empty(aggregate.PendingEvents);
        //Assert.Equal(aggregate.State, new TestState(3, 3, 30));
    }

}