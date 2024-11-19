using EvDb.Core;
using EvDb.StressTests.Outbox;


namespace EvDb.StressTests;

[EvDbAttachView<Views.FaultCount.View>]
[EvDbAttachView<Views.Count.View>("Count")]
[EvDbAttachView<Views.MinInterval.View>("Interval")]
[EvDbStreamFactory<IEvents, StressTestOutbox>("stress", "main")]
public partial class DemoStreamFactory
{
}