using Core.AggregateType;
using Core.Event;
using CoreTests.Event;

namespace CoreTests.AggregateTypeTests
{
    [TestClass]
    public class AggregateTypeTests
    {
        [TestMethod]
        public void AggregateType_WhenAddingEventType_Succeed()
        {
            AggregateType testAggregateType = GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            Assert.IsTrue(testAggregateType.RegisteredEventTypes.TryGetValue(testEventType.EventTypeName, out _));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AggregateType_WhenAddingEventTypeTwice_ThrowException()
        {
            AggregateType testAggregateType = GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            testAggregateType.AddEventType(testEventType);
        }

        public static readonly Type TestStateType = typeof(TestAggregateTypeState);
        public AggregateType GetTestAggregateType()
        {
            return new AggregateType(TestStateType);
        }

    }
    record TestAggregateTypeState(int ACount, int BCount, int BSum);
}