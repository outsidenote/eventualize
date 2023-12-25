using Eventualize.Core.Abstractions.Stream;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public static class TestAggregateFactoryConfigs
{
    public static readonly string AggregateType = "TestAggregateType";
    public static readonly Type TestStateType = typeof(TestState);

    public readonly static EventualizeStreamBaseUri StreamBaseAddress =  new("default", "testStreamType");

    public static EventualizeAggregateFactory<TestState> GetAggregateFactory(int minEvents = 0)
    {
        var map = new Dictionary<string, IFoldingFunction<TestState>>
        {
            [TestEventType] = new TestFoldingFunction()
        };
        EventualizeFoldingLogic<TestState> foldingLogic = new (map);
        EventualizeAggregateFactory<TestState> aggregate =
            new (
                    AggregateType,
                    StreamBaseAddress,
                    foldingLogic,
                    minEvents);


        return aggregate;
    }

    public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();
}