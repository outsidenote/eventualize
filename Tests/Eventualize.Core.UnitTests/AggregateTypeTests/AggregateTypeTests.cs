using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public sealed class AggregateTypeTests
{
    [Fact]
    public void AggregateType_WhenAddingEventType_Succeed()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
        EventualizeEventType testEventType = TestEventType;
        testAggregateType.AddEventType(testEventType);
        Assert.True(testAggregateType.RegisteredEventTypes.TryGetValue(testEventType.EventTypeName, out _));
    }

    [Fact]
    public void AggregateType_WhenAddingEventTypeTwice_ThrowException()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
        EventualizeEventType testEventType = TestEventType;
        testAggregateType.AddEventType(testEventType);
        Assert.Throws<ArgumentException>(() => testAggregateType.AddEventType(testEventType));
    }

    [Fact]
    public void AggregateType_WhenAddingFoldingFunction_Succeed()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
        IFoldingFunction<TestState>? storedFunction;
        Assert.True(testAggregateType.FoldingLogic.TryGetValue(TestEventType.EventTypeName, out storedFunction));
        Assert.True(storedFunction is IFoldingFunction<TestState>);
    }

    [Fact]
    public async Task AggregateType_WhenFoldingEvents_Succeed()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
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
        var (foldedState, count) = await testAggregateType.FoldEventsAsync(
                                        new TestState(), 
                                        events);
        TestState expectedState = new TestState(3, 3, 30);
        Assert.Equal(expectedState, foldedState);
        Assert.Equal(3, count);
    }
}