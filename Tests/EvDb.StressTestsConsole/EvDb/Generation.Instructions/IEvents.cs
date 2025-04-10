using EvDb.Core;

namespace EvDb.StressTests;


[EvDbAttachEventType<FaultOccurred>]
[EvDbAttachEventType<SomethingHappened>]
public partial interface IEvents
{
}

