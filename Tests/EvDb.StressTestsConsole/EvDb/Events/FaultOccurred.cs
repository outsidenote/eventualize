using EvDb.Core;

namespace EvDb.StressTests;

[EvDbDefineEventPayload("fault-occurred")]
public partial record FaultOccurred(int Id, string Name, int Rate);



