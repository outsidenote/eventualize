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
        public static Aggregate<TestState> GetTestAggregate()
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString());
        }
        public static Aggregate<TestState> GetTestAggregate(List<Core.EventEntity> events, bool isPendingEvents)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            if (!isPendingEvents)
                return aggregateType.CreateAggregate(Guid.NewGuid().ToString(), events);
            else
            {
                var aggregate = aggregateType.CreateAggregate(Guid.NewGuid().ToString());
                foreach (var e in events)
                {
                    aggregate.AddPendingEvent(e);
                }
                return aggregate;
            }

        }

        public static Aggregate<TestState> GetTestAggregate(List<Core.EventEntity>? events, int? minEventsBetweenSnapshots)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogicAndMinEvents(minEventsBetweenSnapshots);
            if (events == null)
                return aggregateType.CreateAggregate(Guid.NewGuid().ToString());
            else
                return aggregateType.CreateAggregate(Guid.NewGuid().ToString(), events);
        }

        public static Aggregate<TestState> GetTestAggregate(TestState snapshot, List<Core.EventEntity> events)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            var aggregate = aggregateType.CreateAggregate(Guid.NewGuid().ToString(), snapshot, (long)0, new List<Core.EventEntity>());
            foreach (var e in events)
            {
                aggregate.AddPendingEvent(e);
            }
            return aggregate;
        }
        public static Aggregate<TestState> GetTestAggregateFromStore(TestState snapshot, List<Core.EventEntity> events)
        {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString(), snapshot, (long)0, events);
        }
    }
}