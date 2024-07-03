using EvDb.Core;

namespace EvDb.StressTestsWebApi;


[EvDbEventAdder<FaultOccurred>]
[EvDbEventAdder<SomethingHappened>]
public partial interface IEvents
{
}

