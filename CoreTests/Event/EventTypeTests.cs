namespace CoreTests.Event;
using Core.Event;
using NJsonSchema;

[TestClass]
public class EventTypeTests
{

    [TestMethod]
    public async Task EventType_WhenCreatingEvent_Succeed()
    {
        EventType testEventType = TestEventType;
        TestEventDataType data = new("test", 10);
        await GetCorrectTestEvent();
        return;
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EventType_WhenCreatingEventWithWrongDataType_ThrowException()
    {
        EventType testEventType = TestEventType;
        string data = "wrong data type";
        await testEventType.CreateEvent(data, "TestMethod");
    }

    [TestMethod]
    public async Task EventType_WhenEventDataParsed_Succeed()
    {
        EventType testEventType = TestEventType;
        Event testEvent = await GetCorrectTestEvent();
        TestEventDataType parsedData = testEventType.ParseData(testEvent);
        Assert.AreEqual(CorrectEventData, parsedData);
        Assert.IsInstanceOfType(parsedData, CorrectEventData.GetType());
    }

    public static readonly EventType TestEventType = new EventType("testType", typeof(TestEventDataType));
    public static readonly TestEventDataType CorrectEventData = new("test", 10);
    public async Task<Event> GetCorrectTestEvent()
    {
        return await TestEventType.CreateEvent(CorrectEventData, "TestMethod");
    }
}