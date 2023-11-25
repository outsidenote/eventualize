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
            AggregateType testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            Assert.IsTrue(testAggregateType.RegisteredEventTypes.TryGetValue(testEventType.EventTypeName, out _));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AggregateType_WhenAddingEventTypeTwice_ThrowException()
        {
            AggregateType testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            testAggregateType.AddEventType(testEventType);
        }

        [TestMethod]
        public void AggregateType_WhenAddginFoldingFunction_Succeed()
        {
            AggregateType testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            testAggregateType.AddFoldingFunction(testEventType.EventTypeName, TestAggregateTypeConfigs.TestFoldingFunction);
            FoldingFunction storedFunction;
            Assert.IsTrue(testAggregateType.FoldingLogic.TryGetValue(testEventType.EventTypeName, out storedFunction));
            Assert.AreEqual(typeof(FoldingFunction), storedFunction.GetType());
        }





    }





}