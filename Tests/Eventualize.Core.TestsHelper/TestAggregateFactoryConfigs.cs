using Eventualize.Core.Abstractions.Stream;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    public static class TestAggregateFactoryConfigs
    {
        public static readonly string AggregateType = "TestAggregateType";
        public static readonly Type TestStateType = typeof(TestState);

        public static EventualizeStreamBaseUri GetStreamBaseAddress()
        {
            return new("default", "testStreamType");
        }

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(bool useFoldingLogic2 = false)
        {
            IFoldingFunction<TestState> foldingFunction = !useFoldingLogic2 ?
                new TestFoldingFunction() : new TestFoldingFunction2();

            var foldingLogic = new EventualizeFoldingLogic<TestState>(
                new Dictionary<string, IFoldingFunction<TestState>>(){
                    {TestEventType.EventTypeName, foldingFunction}
                }
            );

            return new(
                AggregateType,
                GetStreamBaseAddress(),
                new() { { TestEventType.EventTypeName, TestEventType } },
                foldingLogic
            );
        }

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(int minEvents)
        {
            return new(
                AggregateType,
                GetStreamBaseAddress(),
                new() { { TestEventType.EventTypeName, TestEventType } },
                new(new() { { TestEventType.EventTypeName, new TestFoldingFunction() } }),
                minEvents
            );
        }

        // public static EventualizeFoldingFunction TestFoldingFunction = new EventualizeFoldingFunction(UndelegatedTestFoldingFunction);

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