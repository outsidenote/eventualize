using Eventualize.Core.AggregateType;

using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
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
            var testEventType = TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static AggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogicAndMinEvents(int? minEvents)
        {
            var aggregate = new AggregateType<TestState>(AggregateTypeName, minEvents ?? 3);
            var testEventType = TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static FoldingFunction TestFoldingFunction = new FoldingFunction(UndelegatedTestFoldingFunction);

        public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();

        private static object UndelegatedTestFoldingFunction(object oldState, EventEntity SerializedEvent)
        {
            TestState convertedOldState = (TestState)oldState;
            EventEntity convertedSerializedEvent = (EventEntity)SerializedEvent;
            TestEventDataType data = TestEventType.ParseData(convertedSerializedEvent);
            return new TestState(convertedOldState.ACount + 1, convertedOldState.BCount + 1, convertedOldState.BSum + data.B);
        }
    }

}