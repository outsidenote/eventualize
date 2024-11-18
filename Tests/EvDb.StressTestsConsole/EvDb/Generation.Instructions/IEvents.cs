using EvDb.Core;

namespace EvDb.StressTests;


[EvDbEventTypes<FaultOccurred>]
[EvDbEventTypes<SomethingHappened>]
public partial interface IEvents
{
}

