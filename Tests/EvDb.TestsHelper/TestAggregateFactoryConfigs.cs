namespace EvDb.Core.Tests;

public static class TestAggregateFactoryConfigs
{
    public static readonly string AggregateType = "TestAggregateType";
    public static readonly string AggregateType2 = "TestAggregateType2";
    public static readonly Type TestStateType = typeof(TestState);

    public static readonly EvDbPartitionAddress GetStreamType = new("default", "testStreamType");
}