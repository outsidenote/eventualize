using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public sealed class AggregateFactoryTests
{
    [Fact]
    public async Task AggregateFactory_WhenFoldingEvents_Succeed()
    {
        async IAsyncEnumerable<EventualizeEvent> GetEventsAsync()
        {
            for (var i = 0; i < 3; i++)
            {
                await Task.Yield();
                EventualizeEvent e = TestEventType.CreateEvent(CorrectEventData, "AggregateType test method");
                yield return e;
            }
        }
        var events = GetEventsAsync();
        var (foldedState, count) = await TestAggregateFactoryConfigs
            .TestAggregateFactory
            .FoldingLogic
            .FoldEventsAsync(events);
        TestState expectedState = new TestState(3, 3, 30);
        Assert.Equal(expectedState, foldedState);
        Assert.Equal(3, count);
    }
}