namespace Eventualize.Core.Tests;
using Eventualize.Core;
using static Eventualize.Core.Tests.TestHelper;

public class EventTypeTests
{

    [Fact]
    public void EventType_WhenCreatingEvent_Succeed()
    {
        EventualizeEventType testEventType = TestEventType;
        TestEventDataType data = new ("test", 10);
        EventualizeEvent e = GetCorrectTestEvent();
    }

    [Fact]
    public void EventType_WhenCreatingEventWithWrongDataType_ThrowException()
    {
        EventualizeEventType testEventType = TestEventType;
        string data = "wrong data type";
        Assert.Throws<ArgumentException>(() =>
            testEventType.CreateEvent(data, "TestOperation"));
    }

    [Fact]
    public void EventType_WhenEventDataParsed_Succeed()
    {
        EventualizeEventType testEventType = TestEventType;
        EventualizeEvent testEvent = GetCorrectTestEvent();
        TestEventDataType parsedData = testEventType.ParseData(testEvent);
        Assert.Equal(CorrectEventData, parsedData);
        Assert.IsType(CorrectEventData.GetType(), parsedData);
    }
}