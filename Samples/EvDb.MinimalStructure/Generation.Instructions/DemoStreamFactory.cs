using EvDb.Core;


namespace EvDb.MinimalStructure;

[EvDbAttachView<Views.A.View>]
[EvDbAttachView<Views.B.View>("MyB")]
[EvDbStreamFactory<IEvents>]
public partial class DemoStreamFactory
{
    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("issues", "view-naming");

    #endregion // PartitionAddress
}