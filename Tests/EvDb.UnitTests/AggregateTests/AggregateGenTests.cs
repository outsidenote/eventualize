namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

using Scenes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Text.Json.Serialization;

public class AggregateGenTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public AggregateGenTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Aggregate_WhenAddingPendingEvent_Succeed()
    {
        IStudentAvg aggregate = AggregateGenSteps
                                            .GivenLocalAggerate(_output)
                                            .WhenAddingPendingEvents();

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            Assert.Single(aggregate.State);
            var studentAvg = aggregate.State.First().Avg;
            Assert.Equal(6, studentAvg);
            Assert.Equal(4, aggregate.EventsCount);
        }
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