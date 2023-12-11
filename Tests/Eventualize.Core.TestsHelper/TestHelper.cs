using Eventualize.Core.Aggregate;

namespace Eventualize.Core.Tests;

public static class TestHelper
{
    public static readonly EventType TestEventType = new EventType("testType", typeof(TestEventDataType));
    public static readonly TestEventDataType CorrectEventData = new("test", 10);
    public static async Task<EventEntity> GetCorrectTestEvent()
    {
        return await TestEventType.CreateEvent(CorrectEventData, "TestOperation");
    }

    public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents()
    {
        var aggregate = TestAggregateConfigs.GetTestAggregate();
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(await GetCorrectTestEvent());
        return aggregate;

    }

    public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
    {
        var aggregate = TestAggregateConfigs.GetTestAggregate(new(), minEventsBetweenSnapshots);
        for (int i = 0; i < 3; i++)
            aggregate.AddPendingEvent(await GetCorrectTestEvent());
        return aggregate;

    }
    public static async Task<Aggregate<TestState>> PrepareAggregateWithEvents()
    {
        List<EventEntity> events = new();
        for (int i = 0; i < 3; i++)
            events.Add(await GetCorrectTestEvent());
        return TestAggregateConfigs.GetTestAggregate(events, true);
    }
}