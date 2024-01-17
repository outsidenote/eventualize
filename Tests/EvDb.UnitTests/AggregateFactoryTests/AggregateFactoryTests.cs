using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class AggregateFactoryTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public AggregateFactoryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithEvents_Succeed()
    {
        var aggregate = await AggregateGenSteps
                        .GivenFactoryForStoredStreamWithEvents(_output)
                        .WhenGetAggregateAsync();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Single(aggregate.State);
            var studentAvg = aggregate.State.First().Value.Sum;
            Assert.Equal(90, studentAvg);
            Assert.Equal(0, aggregate.EventsCount);
        }
    }


    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        //var aggregate = await AggregateGenSteps
        //                .GivenFactoryForStoredStreamWithEvents(_output)
        //                .WhenGetAggregateAsync();

        //ThenStoredEventsAddedSuccessfully();

        //void ThenStoredEventsAddedSuccessfully()
        //{
        //    Assert.Single(aggregate.State);
        //    var studentAvg = aggregate.State.First().Avg;
        //    Assert.Equal(60, studentAvg);
        //    Assert.Equal(0, aggregate.EventsCount);
        //}

        //IAsyncEnumerable<IEvDbStoredEvent> events = TestAggregateConfigs.GetStoredEvents(3);
        //var aggregate = await TestAggregateConfigs.GetTestAggregateAsync(new TestState(3, 3, 30), events);
        //Assert.Empty(aggregate.PendingEvents);
        //Assert.Equal(aggregate.State, new TestState(6, 6, 60));
    }


    [Fact(Skip = "until multi folding")]
    public void AggregateFactory_WhenFoldingEvents_Succeed()
    {
        throw new NotImplementedException();

        //var events = TestAggregateConfigs.GetPendingEvents(3);
        //var (foldedState, count) = TestAggregateFactoryConfigs
        //    .GetAggregateFactory()
        //    .FoldingLogic
        //    .FoldEvents(events);
        //TestState expectedState = new TestState(3, 3, 30);
        //Assert.Equal(expectedState, foldedState);
        //Assert.Equal(3, count);
    }
}