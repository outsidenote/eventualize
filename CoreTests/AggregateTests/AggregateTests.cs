using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.AggregateType;
using CoreTests.AggregateTypeTests;
using CoreTests.Event;
using Core.Event;


namespace CoreTests.AggregateTests
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public async Task Aggregate_WhenAddingPendingEvent_Succeed()
        {
            var aggregate = TestAggregateConfigs.GetTestAggregate(null);
            var e = await EventTypeTests.GetCorrectTestEvent();
            aggregate.AddPendingEvent(e);
            Assert.AreEqual(aggregate.PendingEvents.Count, 1);
            Assert.AreEqual(aggregate.PendingEvents[0], e);
        }

        [TestMethod]
        public async Task Aggregate_WhenInstantiatingWithEvents_Succeed()
        {
            List<Core.Event.Event> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(await EventTypeTests.GetCorrectTestEvent());
            }
            var aggregate = TestAggregateConfigs.GetTestAggregate(events);
            Assert.AreEqual(aggregate.PendingEvents.Count, 0);
            Assert.AreEqual(aggregate.State, new TestState(3, 3, 30));
        }

        [TestMethod]
        public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
        {
            List<Core.Event.Event> events = new();
            for (int i = 0; i < 3; i++)
            {
                events.Add(await EventTypeTests.GetCorrectTestEvent());
            }
            var aggregate = TestAggregateConfigs.GetTestAggregateFromStore(new TestState(3, 3, 30), events);
            Assert.AreEqual(aggregate.PendingEvents.Count, 0);
            Assert.AreEqual(aggregate.State, new TestState(6, 6, 60));
        }

        [TestMethod]
        public void Aggregate_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
        {
            var aggregate = TestAggregateConfigs.GetTestAggregate(new TestState(3, 3, 30), new List<Core.Event.Event>());
            Assert.AreEqual(aggregate.PendingEvents.Count, 0);
            Assert.AreEqual(aggregate.State, new TestState(3, 3, 30));
        }

    }
}