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
        public static Aggregate<TestState> GetTestAggregate() {
            var aggregateType = TestAggregateTypeConfigs.GetTestAggregateTypeWithEventTypeAndFoldingLogic();
            return aggregateType.CreateAggregate(Guid.NewGuid().ToString());
        }
    }
}