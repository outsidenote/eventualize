namespace CoreTests.Event;
using Core.Event;

[TestClass]
public class CapturedEventTests
{
    private CapturedEvent<TestEventDataType>? TestEvent;

    [TestMethod]
    public void CapturedEvent_WhenSerialized_DeserializedSuccessfully()
    {
        TestEventDataType data = new("test", 10);
        TestEvent = new("TestEventType", new DateTime(), "test", data);
        string? serializedEventData = TestEvent.SerializeData();
        TestEventDataType? deserializedData = CapturedEvent<TestEventDataType>.DeserializeData(serializedEventData);
        Assert.AreEqual(data, deserializedData);
    }
}