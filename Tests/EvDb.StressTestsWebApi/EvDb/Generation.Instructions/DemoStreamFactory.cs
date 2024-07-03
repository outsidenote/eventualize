using EvDb.Core;


namespace EvDb.StressTestsWebApi;

[EvDbAttachView<Views.A.View>]
[EvDbAttachView<Views.B.View>("Count")]
[EvDbAttachView<Views.MinInterval.View>("Interval")]
[EvDbStreamFactory<IEvents>]
public partial class DemoStreamFactory
{
    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } =
        new EvDbPartitionAddress("issues", "view-naming");

    #endregion // PartitionAddress
}