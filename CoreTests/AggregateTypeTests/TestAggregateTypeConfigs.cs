using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.AggregateType;
using CoreTests.Event;

namespace CoreTests.AggregateTypeTests
{
    public record TestAggregateTypeState(int ACount = 0, int BCount = 0, int BSum = 0);
    public static class TestAggregateTypeConfigs
    {
        public static readonly Type TestStateType = typeof(TestAggregateTypeState);

        public static AggregateType GetTestAggregateType()
        {
            return new AggregateType(TestStateType);
        }

        public static FoldingFunction TestFoldingFunction = new FoldingFunction(UndelegatedTestFoldingFunction);

        private static object UndelegatedTestFoldingFunction(object oldState, Core.Event.Event SerializedEvent)
        {
            TestAggregateTypeState convertedOldState = (TestAggregateTypeState)oldState;
            Core.Event.Event convertedSerializedEvent = (Core.Event.Event)SerializedEvent;
            TestEventDataType data = EventTypeTests.TestEventType.ParseData(convertedSerializedEvent);
            return new TestAggregateTypeState(convertedOldState.ACount + 1, convertedOldState.BCount + 1, convertedOldState.BSum + data.B);
        }
    }

}