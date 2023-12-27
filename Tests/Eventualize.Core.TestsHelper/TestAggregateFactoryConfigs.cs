using Eventualize.Core;
using static Eventualize.Core.Tests.TestHelper;

namespace Eventualize.Core.Tests;

public static class TestAggregateFactoryConfigs
{
    public static readonly string AggregateType = "TestAggregateType";
    public static readonly string AggregateType2 = "TestAggregateType2";
    public static readonly Type TestStateType = typeof(TestState);

    public readonly static EventualizeStreamBaseUri StreamBaseAddress = new("default", "testStreamType");

    public static EventualizeAggregateFactory<TestState> GetAggregateFactory(
        bool useFoldingLogic2 = false)
    {
        IFoldingFunction<TestState> foldingFunction = !useFoldingLogic2 ?
            new TestFoldingFunction() : new TestFoldingFunction2();

        var foldingLogic = new EventualizeFoldingLogic<TestState>(
            new Dictionary<string, IFoldingFunction<TestState>>()
            {
                [TestEventType] = foldingFunction
            }
        );
        EventualizeAggregateFactory<TestState> aggregate =
            new(
                    AggregateType,
                    StreamBaseAddress,
                    foldingLogic);

        return aggregate;
    }

    public static EventualizeAggregateFactory<TestState> GetAggregateFactory(int minEvents)
    {
        var map = new Dictionary<string, IFoldingFunction<TestState>>
        {
            [TestEventType] = new TestFoldingFunction()
        };
        EventualizeFoldingLogic<TestState> foldingLogic = new(map);
        EventualizeAggregateFactory<TestState> aggregate =
            new(
                    AggregateType,
                    StreamBaseAddress,
                    foldingLogic,
                    minEvents);

        return aggregate;
    }

    public static IFoldingFunction<TestState> FoldingFunctionInstance = new TestFoldingFunction();
}