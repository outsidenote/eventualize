using EvDb.Core;


namespace EvDb.StressTestsWebApi;

[EvDbAttachView<Views.FaultCount.View>]
[EvDbAttachView<Views.Count.View>("Count")]
[EvDbAttachView<Views.MinInterval.View>("Interval")]
[EvDbStreamFactory<IEvents>]
public partial class DemoStreamFactory
{
    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } =
        new EvDbPartitionAddress("stress", "main");

    #endregion // PartitionAddress
}