using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    public static class TestAggregateTypeConfigs
    {
        public static readonly string AggregateTypeName = "TestAggregateType";
        public static readonly Type TestStateType = typeof(TestState);

        public static EventualizeAggregateType<TestState> GetTestAggregateType()
        {
            return new EventualizeAggregateType<TestState>(AggregateTypeName);
        }

        public static EventualizeAggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogic()
        {
            var aggregate = new EventualizeAggregateType<TestState>(AggregateTypeName);
            var testEventType = TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static EventualizeAggregateType<TestState> GetTestAggregateTypeWithEventTypeAndFoldingLogicAndMinEvents(int? minEvents)
        {
            var aggregate = new EventualizeAggregateType<TestState>(AggregateTypeName, minEvents ?? 3);
            var testEventType = TestEventType;
            aggregate.AddEventType(testEventType, FoldingFunctionInstance);
            return aggregate;
        }

        public static EventualizeFoldingFunction TestFoldingFunction = new EventualizeFoldingFunction(UndelegatedTestFoldingFunction);

        public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();

        private static object UndelegatedTestFoldingFunction(object oldState, EventualizeEvent SerializedEvent)
        {
            TestState convertedOldState = (TestState)oldState;
            EventualizeEvent convertedSerializedEvent = (EventualizeEvent)SerializedEvent;
            TestEventDataType data = TestEventType.ParseData(convertedSerializedEvent);
            return new TestState(convertedOldState.ACount + 1, convertedOldState.BCount + 1, convertedOldState.BSum + data.B);
        }
    }

}