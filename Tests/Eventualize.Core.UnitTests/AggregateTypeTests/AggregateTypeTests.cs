using Eventualize.Core.AggregateType;

using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public class AggregateTypeTests
{
    [Fact]
    public void AggregateType_WhenAddingEventType_Succeed()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
        EventType testEventType = TestEventType;
        testAggregateType.AddEventType(testEventType);
        Assert.True(testAggregateType.RegisteredEventTypes.TryGetValue(testEventType.EventTypeName, out _));
    }

    [Fact]
    public void AggregateType_WhenAddingEventTypeTwice_ThrowException()
    {
        var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
        EventType testEventType = TestEventType;
        testAggregateType.AddEventType(testEventType);
        Assert.Throws<ArgumentException>(() => testAggregateType.AddEventType(testEventType));
    }

    [Fact]
    public void AggregateType_WhenAddginFoldingFunction_Succeed()
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
        List<EventEntity> events = new List<EventEntity>();
        for (var i = 0; i < 3; i++)
        {
            events.Add(await TestEventType.CreateEvent(CorrectEventData, "AggregateType test method"));
        }
        TestState foldedState = testAggregateType.FoldEvents(new TestState(), events);
        TestState expectedState = new TestState(3, 3, 30);
        Assert.Equal(foldedState, expectedState);
    }
}