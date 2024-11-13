using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbDefineEventPayload("fault-occurred")]
public partial record FaultOccurred(int Id, string Name, int Rate);



