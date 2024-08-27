using EvDb.Core;


namespace EvDb.UnitTests;

[EvDbAttachView<Issues.Views.A.View>]
[EvDbAttachView<Issues.Views.Count.View>("Count")]
[EvDbAttachView<Issues.Views.CourceCreated.View>("Courses")]
[EvDbStreamFactory<IEvDbSchoolStreamAdders>("issues", "view-naming")]
public partial class IssueStreamFactory
{
}