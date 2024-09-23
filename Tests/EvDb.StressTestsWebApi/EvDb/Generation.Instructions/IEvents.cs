using EvDb.Core;

namespace EvDb.StressTestsWebApi;


[EvDbEventTypes<FaultOccurred>]
[EvDbEventTypes<SomethingHappened>]
public partial interface IEvents
{
}

