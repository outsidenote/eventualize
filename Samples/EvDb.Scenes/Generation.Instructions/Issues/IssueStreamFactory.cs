using EvDb.Core;


namespace EvDb.UnitTests;

[EvDbAttachView<Issues.Views.A.View>]
[EvDbAttachView<Issues.Views.Count.View>("Count")]
[EvDbAttachView<Issues.Views.CourceCreated.View>("Courses")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>]
public partial class IssueStreamFactory
{
    #region Partition

    public override EvDbPartitionAddress PartitionAddress { get; } = new EvDbPartitionAddress("issues", "view-naming");

    #endregion // PartitionAddress
}