using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

using STATE_TYPE = System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>;

public interface IFoo
{
    Task<Bar<T>?> Get<T>(int x, string? s = null);
}

public record Bar<T>(string Key, T Value);



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
        var aggregate = await Steps
                        .GivenFactoryForStoredStreamWithEvents(_output, _storageAdapter)
                        .GivenNoSnapshot(_storageAdapter)
                        .WhenGetAggregateAsync();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Single(aggregate.State);
            var studentAvg = aggregate.State.First().Value.Sum;
            Assert.Equal(180, studentAvg);
            Assert.Equal(0, aggregate.EventsCount);
        }
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output, false);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Single(aggregate.State);
            var studentSum = aggregate.State.First().Value.Sum;
            Assert.Equal(70, studentSum);
            Assert.Equal(0, aggregate.EventsCount);
        }
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Single(aggregate.State);
            var studentSum = aggregate.State.First().Value.Sum;
            Assert.Equal(250, studentSum);
            Assert.Equal(0, aggregate.EventsCount);
        }
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