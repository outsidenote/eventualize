using EvDb.Core;

namespace EvDb.StressTestsWebApi;


[EvDbEventAdder<Event1>]
[EvDbEventAdder<Event2>]
public partial interface IEvents
{
}

