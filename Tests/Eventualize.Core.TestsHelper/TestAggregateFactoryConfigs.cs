using System.Collections.Immutable;
using Eventualize.Core;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    public static class TestAggregateFactoryConfigs
    {
        public static readonly string AggregateType = "TestAggregateType";
        public static readonly string AggregateType2 = "TestAggregateType2";
        public static readonly Type TestStateType = typeof(TestState);

        public static EventualizeStreamBaseUri GetStreamBaseAddress()
        {
            return new("default", "testStreamType");
        }

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(bool useFoldingLogic2 = false)
        {
            return GetAggregateFactory(0, useFoldingLogic2);
        }

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(int minEvents, bool useFoldingLogic2 = false)
        {
            IFoldingFunction<TestState> foldingFunction = !useFoldingLogic2 ?
                new TestFoldingFunction() : new TestFoldingFunction2();
            var map = new KeyValuePair<string, IFoldingFunction<TestState>>[] {
                KeyValuePair.Create(TestEventType.EventTypeName, foldingFunction)
            };

            var foldingLogic = new EventualizeFoldingLogic<TestState>(ImmutableDictionary.CreateRange(map));

            return new(
                useFoldingLogic2 ? AggregateType2 : AggregateType,
                GetStreamBaseAddress(),
                new() { { TestEventType.EventTypeName, TestEventType } },
                foldingLogic,
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