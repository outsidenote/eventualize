namespace CoreTests.Event;
using Core.Event;
using NJsonSchema;

[TestClass]
public class EventTypeTests
{

    [TestMethod]
    public async Task EventType_WhenCreatingEvent_Succeed()
    {
        EventType testEventType = new EventType("testType", typeof(TestEventDataType));
        TestEventDataType data = new("test", 10);
        await testEventType.CreateEvent(data, "TestMethod");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EventType_WhenCreatingEventWithWrongDataType_ThrowException()
    {
        EventType testEventType = new EventType("testType", typeof(TestEventDataType));
        string data = "wrong data type";
        await testEventType.CreateEvent(data, "TestMethod");
    }

    [TestMethod]
    public async Task EventType_WhenEventDataParsed_Succeed()
    {
        EventType testEventType = new EventType("testType", typeof(TestEventDataType));
        TestEventDataType data = new("test", 10);
        Event testEvent = await testEventType.CreateEvent(data, "TestMethod");
        TestEventDataType parsedData = testEventType.ParseData(testEvent);
        Assert.AreEqual(data, parsedData);
        Assert.IsInstanceOfType(parsedData, data.GetType());
    }

}