using EvDb.Core;

namespace EvDb.MinimalStructure;


[EvDbEventAdder<Event1>]
[EvDbEventAdder<Event2>]
public partial interface IEvents
{
}

