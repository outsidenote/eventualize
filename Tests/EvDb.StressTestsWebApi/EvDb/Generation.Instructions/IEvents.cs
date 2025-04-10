using EvDb.Core;

namespace EvDb.StressTestsWebApi;


[EvDbAttachEventType<FaultOccurred>]
[EvDbAttachEventType<SomethingHappened>]
public partial interface IEvents
{
}

