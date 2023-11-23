namespace CoreTests.Event;
using Core.Event;

[TestClass]
public class EventTests
{
    private Event<TestEventDataType>? TestEvent;

    [TestMethod]
    public void Event_WhenSerialized_DeserializedSuccessfully()
    {
        TestEventDataType data = new("test", 10);
        TestEvent = new("TestEventType", new DateTime(), "test", data, new DateTime());
        string? serializedEventData = TestEvent.SerializeData();
        TestEventDataType? deserializedData = Event<TestEventDataType>.DeserializeData(serializedEventData);
        Assert.AreEqual(data, deserializedData);
    }
}