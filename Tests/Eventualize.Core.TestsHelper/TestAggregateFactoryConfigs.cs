using System.Collections.Immutable;
using Eventualize.Core;
using Eventualize.Core.Abstractions;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests
{
    public static class TestAggregateFactoryConfigs
    {
        public static readonly string AggregateType = "TestAggregateType";
        public static readonly string AggregateType2 = "TestAggregateType2";
        public static readonly Type TestStateType = typeof(TestState);

        public static readonly EventualizeStreamBaseUri GetStreamBaseUri = new("default", "testStreamType");

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(bool useFoldingLogic2 = false)
        {
            return GetAggregateFactory(0, useFoldingLogic2);
        }

        public static EventualizeAggregateFactory<TestState> GetAggregateFactory(int minEvents, bool useFoldingLogic2 = false)
        {
            IFoldingFunction<TestState> foldingFunction = !useFoldingLogic2 ?
                new TestFoldingFunction() : new TestFoldingFunction2();
            
            var foldingLogic = EventualizeFoldingLogicBuilder.Create<TestState>()
                            .AddMapping(TestEventType, foldingFunction)
                            .Build();

            EventualizeStreamBaseUri streamBaseUri =  GetStreamBaseUri;
            return new EventualizeAggregateFactory<TestState>(
                useFoldingLogic2 ? AggregateType2 : AggregateType, 
                streamBaseUri,                
                foldingLogic,
                minEvents
            );
        }
    }
}