using EvDb.Core;


namespace EvDb.MinimalStructure;

[EvDbAttachView<Views.A.View>]
[EvDbAttachView<Views.B.View>("Count")]
[EvDbAttachView<Views.MinInterval.View>("Interval")]
[EvDbStreamFactory<IEvents>("issues:view-naming")]
public partial class DemoStreamFactory
{
}