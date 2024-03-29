namespace EvDb.Core.Tests;

public static class TestAggregateFactoryConfigs
{
    public static readonly string AggregateType = "TestAggregateType";
    public static readonly string AggregateType2 = "TestAggregateType2";
    public static readonly Type TestStateType = typeof(TestState);

    public static readonly EvDbPartitionAddress GetStreamType = new("default", "testStreamType");

    //public static EvDbAggregateFactory<TestState> GetAggregateFactory(bool useFoldingLogic2 = false)
    //{
    //    return GetAggregateFactory(0, useFoldingLogic2);
    //}

    //public static EvDbAggregateFactory<TestState> GetAggregateFactory(int minEvents, bool useFoldingLogic2 = false)
    //{
    //    IFoldingFunction<TestState> foldingFunction = !useFoldingLogic2 ?
    //        new TestFoldingFunction() : new TestFoldingFunction2();

    //    var foldingLogic = EvDbFoldingLogicBuilder.Create<TestState>()
    //                    .AddMapping(TestEventType, foldingFunction)
    //                    .Build();

    //    EvDbPartitionAddress streamType = GetStreamType;
    //    return new EvDbAggregateFactory<TestState>(
    //        useFoldingLogic2 ? AggregateType2 : AggregateType,
    //        streamType,
    //        foldingLogic,
    //        minEvents
    //    );
    //}
}