namespace Eventualize.Core.Tests;
using Eventualize.Core;
using static Eventualize.Core.Tests.TestHelper;

public class EventTypeTests
{

    [Fact]
    public async Task EventType_WhenCreatingEvent_Succeed()
    {
        EventType testEventType = TestEventType;
        TestEventDataType data = new("test", 10);
        await GetCorrectTestEvent();
        return;
    }

    [Fact]
    public async Task EventType_WhenCreatingEventWithWrongDataType_ThrowException()
    {
        EventType testEventType = TestEventType;
        string data = "wrong data type";
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await testEventType.CreateEvent(data, "TestOperation"));
    }

    [Fact]
    public async Task EventType_WhenEventDataParsed_Succeed()
    {
        EventType testEventType = TestEventType;
        EventEntity testEvent = await GetCorrectTestEvent();
        TestEventDataType parsedData = testEventType.ParseData(testEvent);
        Assert.Equal(CorrectEventData, parsedData);
        Assert.IsType(CorrectEventData.GetType(), parsedData);
    }
}