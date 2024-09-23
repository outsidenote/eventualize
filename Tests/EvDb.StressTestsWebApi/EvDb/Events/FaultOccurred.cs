using EvDb.Core;

namespace EvDb.StressTestsWebApi;

[EvDbDefinePayload("fault-occurred")]
public partial record FaultOccurred(int Id, string Name, int Rate);



