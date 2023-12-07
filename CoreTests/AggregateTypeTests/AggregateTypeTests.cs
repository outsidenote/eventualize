using System.Security.Cryptography.X509Certificates;
using Core.AggregateType;
using Core;
using CoreTests.Event;

namespace CoreTests.AggregateTypeTests
{
    [TestClass]
    public class AggregateTypeTests
    {
        [TestMethod]
        public void AggregateType_WhenAddingEventType_Succeed()
        {
            var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            Assert.IsTrue(testAggregateType.RegisteredEventTypes.TryGetValue(testEventType.EventTypeName, out _));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AggregateType_WhenAddingEventTypeTwice_ThrowException()
        {
            var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateType();
            EventType testEventType = EventTypeTests.TestEventType;
            testAggregateType.AddEventType(testEventType);
            testAggregateType.AddEventType(testEventType);
        }

        [TestMethod]
        public void AggregateType_WhenAddginFoldingFunction_Succeed()
        {
            var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            IFoldingFunction<TestState>? storedFunction;
            Assert.IsTrue(testAggregateType.FoldingLogic.TryGetValue(EventTypeTests.TestEventType.EventTypeName, out storedFunction));
            Assert.IsTrue(storedFunction is IFoldingFunction<TestState>);
        }

        [TestMethod]
        public async Task AggregateType_WhenFoldingEvents_Succeed()
        {
            var testAggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            List<Core.EventEntity> events = new List<Core.EventEntity>();
            for (var i = 0; i < 3; i++)
            {
                events.Add(await EventTypeTests.TestEventType.CreateEvent(EventTypeTests.CorrectEventData, "AggregateType test method"));
            }
            TestState foldedState = testAggregateType.FoldEvents(new TestState(), events);
            TestState expectedState = new TestState(3, 3, 30);
            Assert.AreEqual(foldedState, expectedState);
        }
    }
}