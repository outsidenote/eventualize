using EvDb.Core;


namespace EvDb.StressTestsWebApi;

[EvDbAttachView<Views.FaultCount.View>]
[EvDbAttachView<Views.Count.View>("Count")]
[EvDbAttachView<Views.MinInterval.View>("Interval")]
[EvDbStreamFactory<IEvents>("stress", "main")]
public partial class DemoStreamFactory
{
}