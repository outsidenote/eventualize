namespace CoreTests.Event;
using Core.Event;

[TestClass]
public class StoredEventTests
{
    private StoredEvent<TestEventDataType>? TestEvent;

    [TestMethod]
    public void StoredEvent_WhenSerialized_DeserializedSuccessfully()
    {
        TestEventDataType data = new("test", 10);
        TestEvent = new("TestEventType", new DateTime(), "test", data, new DateTime());
        string? serializedEventData = TestEvent.SerializeData();
        TestEventDataType? deserializedData = StoredEvent<TestEventDataType>.DeserializeData(serializedEventData);
        Assert.AreEqual(data, deserializedData);
    }
}