using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using CoreTests.AggregateTypeTests;

namespace CoreTests.AggregateTests
{
    public class TestAggregateConfigs
    {
        public static Aggregate<TestState> GetTestAggregate(List<Core.Event.Event>? events)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString(), events);
        }

        public static Aggregate<TestState> GetTestAggregate(TestState snapshot, List<Core.Event.Event> events)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            var aggregate = aggregateType.CreateAggregate(Guid.NewGuid().ToString(), snapshot, (long)0, new List<Core.Event.Event>());
            foreach (var e in events)
            {
                aggregate.AddPendingEvent(e);
            }
            return aggregate;
        }
        public static Aggregate<TestState> GetTestAggregateFromStore(TestState snapshot, List<Core.Event.Event> events)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString(), snapshot, (long)0, events);
        }
    }
}