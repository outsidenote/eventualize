using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbEventPayload("fault-occurred")]
public partial record FaultOccurred(int Id, string Name, int Rate);



