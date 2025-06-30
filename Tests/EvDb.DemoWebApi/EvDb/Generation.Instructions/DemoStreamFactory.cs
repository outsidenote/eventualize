using EvDb.Core;
using EvDb.DemoWebApi.Outbox;


namespace EvDb.DemoWebApi;

[EvDbAttachView<Views.Count.View>("Status")]
[EvDbStreamFactory<IEvents, DemoOutbox>("demo:main")]
public partial class DemoStreamFactory
{
}