using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Eventualize.Core.AggregateType;
using CoreTests.Event;
using System.Text.Json.Serialization;
using Eventualize.Core;
using Generator.Equals;

namespace CoreTests.AggregateTypeTests
{
    [Equatable]
    public partial class TestState
    {
        public int ACount { get; private set; }
        public int BCount { get; private set; }
        public int BSum { get; private set; }

        public TestState()
        {
            ACount = 0;
            BCount = 0;
            BSum = 0;

        }

        [JsonConstructor]
        public TestState(int aCount, int bCount, int bSum)
        {
            ACount = aCount;
            BCount = bCount;
            BSum = bSum;
        }

        public override string ToString()
        {
            return $"(ACount={ACount}, BCount={BCount}, BSum={BSum})";
        }
    };
    public static class TestAggregateTypeConfigs
    {
        public static readonly string AggregateTypeName = "TestAggregateType";
        public static readonly Type TestStateType = typeof(TestState);

        public static AggregateType<TestState> GetTestAggregateType()
        {
            return new AggregateType<TestState>(AggregateTypeName);
        }

        public static AggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogic()
        {
            var aggregate = new AggregateType<TestState>(AggregateTypeName);
            var testEventType = EventTypeTests.TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static AggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogicAndMinEvents(int? minEvents)
        {
            var aggregate = new AggregateType<TestState>(AggregateTypeName, minEvents ?? 3);
            var testEventType = EventTypeTests.TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static FoldingFunction TestFoldingFunction = new FoldingFunction(UndelegatedTestFoldingFunction);

        public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();

        private static object UndelegatedTestFoldingFunction(object oldState, EventEntity SerializedEvent)
        {
            TestState convertedOldState = (TestState)oldState;
            EventEntity convertedSerializedEvent = (EventEntity)SerializedEvent;
            TestEventDataType data = EventTypeTests.TestEventType.ParseData(convertedSerializedEvent);
            return new TestState(convertedOldState.ACount + 1, convertedOldState.BCount + 1, convertedOldState.BSum + data.B);
        }
    }

    class TestFoldingFunction : IFoldingFunction<TestState>
    {
        public TestState Fold(TestState oldState, EventEntity SerializedEvent)
        {
            TestEventDataType data = EventTypeTests.TestEventType.ParseData(SerializedEvent);
            return new TestState(oldState.ACount + 1, oldState.BCount + 1, oldState.BSum + data.B);
        }

    }

}