using EvDb.Core;


namespace EvDb.UnitTests;

[EvDbAttachView<Issues.Views.A.View>]
[EvDbAttachView<Issues.Views.B.View>("MyB")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>]
public partial class IssueStreamFactory
{
    #region Ctor

    public IssueStreamFactory(IEvDbStorageAdapter storageAdapter) : base(storageAdapter)
    {
    }

    #endregion // Ctor

    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("issues", "view-naming");

    #endregion // PartitionAddress
}