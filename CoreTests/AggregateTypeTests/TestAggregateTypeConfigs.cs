using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Core.AggregateType;
using CoreTests.Event;
using System.Text.Json;

namespace CoreTests.AggregateTypeTests
{
    public class TestState : IEquatable<TestState>
    {
        public readonly int ACount = 0;
        public readonly int BCount = 0;
        public readonly int BSum = 0;

        public TestState()
        {
            ACount = 0;
            BCount = 0;
            BSum = 0;

        }

        public TestState(int aCount, int bCount, int bSum)
        {
            ACount = aCount;
            BCount = bCount;
            BSum = bSum;
        }

        public bool Equals(TestState? other)
        {
            if (other == null) return false;
            return ACount == other.ACount && BCount == other.BCount && BSum == other.BSum;
        }

        public override string ToString()
        {
            return $"(ACount={ACount}, BCount={BCount}, BSum={BSum})";
        }
    };
    public static class TestAggregateTypeConfigs
    {
        public static readonly Type TestStateType = typeof(TestState);

        public static AggregateType<TestState> GetTestAggregateType()
        {
            return new AggregateType<TestState>();
        }

        public static AggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogic()
        {
            var aggregate = new AggregateType<TestState>();
            var testEventType = EventTypeTests.TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static FoldingFunction TestFoldingFunction = new FoldingFunction(UndelegatedTestFoldingFunction);

        public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();

        private static object UndelegatedTestFoldingFunction(object oldState, Core.Event.Event SerializedEvent)
        {
            TestState convertedOldState = (TestState)oldState;
            Core.Event.Event convertedSerializedEvent = (Core.Event.Event)SerializedEvent;
            TestEventDataType data = EventTypeTests.TestEventType.ParseData(convertedSerializedEvent);
            return new TestState(convertedOldState.ACount + 1, convertedOldState.BCount + 1, convertedOldState.BSum + data.B);
        }
    }

    class TestFoldingFunction : IFoldingFunction<TestState>
    {
        public TestState Fold(TestState oldState, Core.Event.Event SerializedEvent)
        {
            TestEventDataType data = EventTypeTests.TestEventType.ParseData(SerializedEvent);
            return new TestState(oldState.ACount + 1, oldState.BCount + 1, oldState.BSum + data.B);
        }

    }

}