// namespace CoreTests.Event;
// using Core.Event;

// [TestClass]
// public class EventTests
// {

//     [TestMethod]
//     public void Event_WhenSerialized_DeserializedSuccessfully()
//     {
//         TestEventDataType data = new("test", 10);
//         Event<TestEventDataType> testEvent = new(new DateTime(), "test", data, new DateTime());
//         string serializedEvent = testEvent.Serialize();
//         Event<TestEventDataType>? deserializedEvent = Event<TestEventDataType>.Deserialize(serializedEvent);
//         Assert.AreEqual(testEvent, deserializedEvent);
//     }
// }